using System.Text.Json;
using API.Entities;
using API.Interfaces;

namespace JobAssistantSystem.Backend.API.Services;

public sealed class EmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    private const string EmbedJobsPath = "/embed/jobs";

    public EmbeddingService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.Timeout = TimeSpan.FromSeconds(300);

        var baseUrl =
            Environment.GetEnvironmentVariable("EMBEDDING_SERVICE_BASEURL") ??
            "http://localhost:5001";

        _httpClient.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
    }

    public async Task<EmbeddingEntity[]> EmbedJobsAsync(IEnumerable<MatchingObject> jobs, CancellationToken cancellationToken = default)
    {
        if (jobs is null)
            throw new ArgumentNullException(nameof(jobs));

        var payload = jobs as MatchingObject[] ?? jobs.ToArray();
        if (payload.Length == 0)
            return Array.Empty<EmbeddingEntity>();

        using var response = await _httpClient.PostAsJsonAsync(
            EmbedJobsPath,
            payload,
            _jsonOptions,
            cancellationToken
        ).ConfigureAwait(false);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            throw new HttpRequestException($"Embedding service error {(int)response.StatusCode}: {body}");
        }

        var embeddings = await response.Content
            .ReadFromJsonAsync<EmbeddingEntity[]>(_jsonOptions, cancellationToken)
            .ConfigureAwait(false);

        return embeddings ?? Array.Empty<EmbeddingEntity>();
    }
}
