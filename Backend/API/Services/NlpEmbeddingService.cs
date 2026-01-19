using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using API.Interfaces;

namespace JobAssistantSystem.Backend.API.Services;

public sealed class NlpEmbeddingService : INlpEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    private const string NlpEmbedPath = "/nlp-embed";

    public NlpEmbeddingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
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

        using var resp = await _httpClient.PostAsJsonAsync(
            NlpEmbedPath,
            new { prompt = text },
            _jsonOptions,
            ct
        ).ConfigureAwait(false);

        if (!resp.IsSuccessStatusCode)
        {
            var body = await resp.Content.ReadAsStringAsync(ct).ConfigureAwait(false);
            throw new HttpRequestException($"NLP-Embedding orchestrator error {(int)resp.StatusCode}: {body}");
        }

        var wrapper = await resp.Content
            .ReadFromJsonAsync<NlpEmbedResponse>(_jsonOptions, ct)
            .ConfigureAwait(false);

        return wrapper?.Response ?? Array.Empty<MatchingWithEmbeddingDTO>();
    }

    private sealed class NlpEmbedResponse
    {
        [JsonPropertyName("response")]
        public MatchingWithEmbeddingDTO[] Response { get; set; } = Array.Empty<MatchingWithEmbeddingDTO>();
    }
}