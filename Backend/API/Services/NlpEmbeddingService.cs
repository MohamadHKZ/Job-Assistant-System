using System;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;
using API.Interfaces;
using API.Logging;
using JobAssistantSystem.API.Errors;
using Microsoft.Extensions.Logging;

namespace JobAssistantSystem.Backend.API.Services;

public sealed class NlpEmbeddingService : INlpEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NlpEmbeddingService> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    private const string NlpEmbedPath = "/nlp-embed";

    public NlpEmbeddingService(HttpClient httpClient, ILogger<NlpEmbeddingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _httpClient.Timeout = TimeSpan.FromSeconds(300);

        var baseUrl =
            Environment.GetEnvironmentVariable("NLP_EMBEDDING_ORCHESTRATOR_BASEURL") ??
            "http://localhost:5003";

        _httpClient.BaseAddress = new Uri(baseUrl!, UriKind.Absolute);
    }

    public async Task<MatchingWithEmbeddingDTO[]> StructureAndEmbed(string text)
        => await MatchWithEmbeddingsAsync(text, CancellationToken.None);

    private async Task<MatchingWithEmbeddingDTO[]> MatchWithEmbeddingsAsync(string text, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(text))
            return Array.Empty<MatchingWithEmbeddingDTO>();

        var sw = Stopwatch.StartNew();
        using var resp = await _httpClient.PostAsJsonAsync(
            NlpEmbedPath,
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
                "NLP-Embedding orchestrator returned {Status} for prompt_len={PromptLen} in {ElapsedMs}ms. Body: {Body}",
                (int)resp.StatusCode, text.Length, sw.ElapsedMilliseconds, safe);
            if (_logger.IsEnabled(LogLevel.Debug) && body.Length > 500)
                _logger.LogDebug("Full NLP-Embedding orchestrator error body: {Body}", body);

            throw new UpstreamServiceException($"NLP-Embedding orchestrator returned {(int)resp.StatusCode}.");
        }

        var wrapper = await resp.Content
            .ReadFromJsonAsync<NlpEmbedResponse>(_jsonOptions, ct)
            .ConfigureAwait(false);

        _logger.LogInformation(
            "NLP-Embedding orchestrator OK: prompt_len={PromptLen} in {ElapsedMs}ms",
            text.Length, sw.ElapsedMilliseconds);

        return wrapper?.Response ?? Array.Empty<MatchingWithEmbeddingDTO>();
    }

    private sealed class NlpEmbedResponse
    {
        [JsonPropertyName("response")]
        public MatchingWithEmbeddingDTO[] Response { get; set; } = Array.Empty<MatchingWithEmbeddingDTO>();
    }
}
