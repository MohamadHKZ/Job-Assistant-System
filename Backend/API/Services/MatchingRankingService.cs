using System.Diagnostics;
using System.Net.Http.Json;
using API.DTOs;
using API.Logging;
using JobAssistantSystem.API.Errors;
using Microsoft.Extensions.Logging;

namespace API.Services;

public class MatchingRankingService : IMatchingRankingService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<MatchingRankingService> _logger;
    private const string BaseUrlEnvVar = "MATCHING_SERVICE_BASE_URL";

    public MatchingRankingService(HttpClient httpClient, ILogger<MatchingRankingService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

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

        var sw = Stopwatch.StartNew();
        using var response = await _httpClient.PostAsJsonAsync("/match/jobs", payload, cancellationToken);
        sw.Stop();

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            var safe = LogText.Truncate500(body);
            _logger.LogError(
                "Matching service returned {Status} ({Reason}) for jobs={JobCount} in {ElapsedMs}ms. Body: {Body}",
                (int)response.StatusCode, response.ReasonPhrase, jobs.Count, sw.ElapsedMilliseconds, safe);
            if (_logger.IsEnabled(LogLevel.Debug) && body.Length > 500)
                _logger.LogDebug("Full matching-service error body: {Body}", body);

            throw new UpstreamServiceException($"Matching service returned {(int)response.StatusCode}.");
        }

        var results = await response.Content.ReadFromJsonAsync<MatchResultDTO[]>(cancellationToken: cancellationToken);
        _logger.LogInformation(
            "Matching service OK: jobs={JobCount} in {ElapsedMs}ms",
            jobs.Count, sw.ElapsedMilliseconds);

        return results ?? Array.Empty<MatchResultDTO>();
    }

    public async Task<List<MatchResultDTO>> Rank(MatchingObjectDTO profile, List<MatchingObjectDTO> jobs)
    {
        var results = await MatchJobsAsync(jobs, profile);
        return results.ToList();
    }
}
