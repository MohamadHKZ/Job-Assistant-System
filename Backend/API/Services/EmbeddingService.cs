using System.Diagnostics;
using System.Text.Json;
using API.Entities;
using API.Interfaces;
using API.Logging;
using JobAssistantSystem.API.Errors;
using Microsoft.Extensions.Logging;

namespace JobAssistantSystem.Backend.API.Services;

public sealed class EmbeddingService : IEmbeddingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmbeddingService> _logger;
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    private const string EmbedJobsPath = "/embed/jobs";

    public EmbeddingService(HttpClient httpClient, ILogger<EmbeddingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
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

        var sw = Stopwatch.StartNew();
        using var response = await _httpClient.PostAsJsonAsync(
            EmbedJobsPath,
            payload,
            _jsonOptions,
            cancellationToken
        ).ConfigureAwait(false);
        sw.Stop();

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            var safe = LogText.Truncate500(body);
            _logger.LogError(
                "Embedding service returned {Status} for {Count} items in {ElapsedMs}ms. Body: {Body}",
                (int)response.StatusCode, payload.Length, sw.ElapsedMilliseconds, safe);
            if (_logger.IsEnabled(LogLevel.Debug) && body.Length > 500)
                _logger.LogDebug("Full embedding-service error body: {Body}", body);

            throw new UpstreamServiceException($"Embedding service returned {(int)response.StatusCode}.");
        }

        var embeddings = await response.Content
            .ReadFromJsonAsync<EmbeddingEntity[]>(_jsonOptions, cancellationToken)
            .ConfigureAwait(false);

        _logger.LogInformation(
            "Embedding service OK: {Count} items in {ElapsedMs}ms",
            payload.Length, sw.ElapsedMilliseconds);

        return embeddings ?? Array.Empty<EmbeddingEntity>();
    }
}
