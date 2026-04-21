using System.Net.Http.Json;
using API.DTOs;

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
        List<MatchingObjectDTO> jobs,
        MatchingObjectDTO profile,
        CancellationToken cancellationToken = default)
    {
        if (jobs is null) throw new ArgumentNullException(nameof(jobs));
        if (profile is null) throw new ArgumentNullException(nameof(profile));

        var payload = new
        {
            jobs,
            profile
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

    public async Task<List<MatchResultDTO>> Rank(MatchingObjectDTO profile, List<MatchingObjectDTO> jobs)
    {
        var results = await MatchJobsAsync(jobs, profile);
        return results.ToList();
    }
}
