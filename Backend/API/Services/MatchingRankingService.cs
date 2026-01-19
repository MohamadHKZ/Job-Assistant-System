using System.Net.Http.Json;
using API.Entities;

namespace API.Services;

public class MatchingRankingService : IMatchingRankingService
{
    private readonly HttpClient _httpClient;
    private const string BaseUrlEnvVar = "MATCHING_SERVICE_BASE_URL";

    public MatchingRankingService(HttpClient httpClient)
    {
        _httpClient = httpClient;

        // Take the URL from environment variables
        if (_httpClient.BaseAddress is null)
        {
            var baseUrl = Environment.GetEnvironmentVariable(BaseUrlEnvVar) ??
            "http://localhost:5002";
            if (string.IsNullOrWhiteSpace(baseUrl))
                throw new InvalidOperationException($"Environment variable '{BaseUrlEnvVar}' is not set.");

            _httpClient.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
        }
    }

    public async Task<MatchResultDTO[]> MatchJobsAsync(
        List<EmbeddingEntity> jobEmbeddings,
        EmbeddingEntity profileEmbedding,
        CancellationToken cancellationToken = default)
    {
        if (jobEmbeddings is null) throw new ArgumentNullException(nameof(jobEmbeddings));
        if (profileEmbedding is null) throw new ArgumentNullException(nameof(profileEmbedding));

        var payload = new
        {
            job_embeddings = jobEmbeddings,
            profile_embedding = profileEmbedding
        };

        using var response = await _httpClient.PostAsJsonAsync("/match/jobs", payload, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            throw new HttpRequestException(
                $"Matching service returned {(int)response.StatusCode} ({response.ReasonPhrase}). Body: {body}");
        }

        var results = await response.Content.ReadFromJsonAsync<MatchResultDTO[]>(cancellationToken: cancellationToken);
        return results ?? Array.Empty<MatchResultDTO>();
    }

    public async Task<List<MatchResultDTO>> Rank(EmbeddingEntity ProfileEmbedding, List<EmbeddingEntity> JobEmbeddings)
    {
        var results = await MatchJobsAsync(JobEmbeddings, ProfileEmbedding);
        return results.ToList();
    }
}
