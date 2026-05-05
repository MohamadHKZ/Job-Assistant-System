using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using API.Entities;
using API.Logging;
using JobAssistantSystem.API.Errors;
using JobAssistantSystem.Backend.API.Interfaces;
using Microsoft.Extensions.Logging;

namespace JobAssistantSystem.Backend.API.Services;

public sealed class NlpService : INlpService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NlpService> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };
    private const string AskPath = "/llm/ask";

    public NlpService(HttpClient httpClient, ILogger<NlpService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.Timeout = TimeSpan.FromSeconds(300);

        var baseUrl =
            Environment.GetEnvironmentVariable("NLP_SERVICE_BASEURL") ??
            "http://localhost:8000";

        _httpClient.BaseAddress = new Uri(baseUrl!, UriKind.Absolute);
    }

    public ProfileDTO StructureProfile(string text)
        => PromptAsync(text, CancellationToken.None).GetAwaiter().GetResult();

    private async Task<ProfileDTO> PromptAsync(string text, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(text))
            return new ProfileDTO();

        var sw = Stopwatch.StartNew();
        using var resp = await _httpClient.PostAsJsonAsync(
            AskPath,
            new { prompt = text },
            _jsonOptions,
            ct
        ).ConfigureAwait(false);
        sw.Stop();

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            var safe = LogText.Truncate500(body);
            _logger.LogError(
                "NLP service returned {Status} for prompt_len={PromptLen} in {ElapsedMs}ms. Body: {Body}",
                (int)resp.StatusCode, text.Length, sw.ElapsedMilliseconds, safe);
            if (_logger.IsEnabled(LogLevel.Debug) && body.Length > 500)
                _logger.LogDebug("Full NLP service error body: {Body}", body);

            throw new UpstreamServiceException($"NLP service returned {(int)resp.StatusCode}.");
        }

        var wrapper = await resp.Content.ReadFromJsonAsync<LlmAskResponse>(_jsonOptions, ct).ConfigureAwait(false);
        _logger.LogInformation(
            "NLP service OK: prompt_len={PromptLen} in {ElapsedMs}ms",
            text.Length, sw.ElapsedMilliseconds);

        if (wrapper is null)
            return new ProfileDTO();

        var payload = wrapper.Response;

        if (payload.ValueKind == JsonValueKind.Object)
        {
            if (payload.TryGetProperty("matches", out var matches) && matches.ValueKind == JsonValueKind.Array)
            {
                var arr = matches.Deserialize<ProfileDTO[]>(_jsonOptions);
                return (arr is { Length: > 0 }) ? arr[0] : new ProfileDTO();
            }

            return payload.Deserialize<ProfileDTO>(_jsonOptions) ?? new ProfileDTO();
        }

        if (payload.ValueKind == JsonValueKind.Array)
        {
            var arr = payload.Deserialize<ProfileDTO[]>(_jsonOptions);
            return (arr is { Length: > 0 }) ? arr[0] : new ProfileDTO();
        }

        return new ProfileDTO();
    }

    private sealed class LlmAskResponse
    {
        [JsonPropertyName("response")]
        public JsonElement Response { get; set; }
    }
}
