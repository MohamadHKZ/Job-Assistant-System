using API.DTOs;

namespace API.Tests.TestDoubles;

public sealed class TestMatchingRankingService : IMatchingRankingService
{
    public Task<List<MatchResultDTO>> Rank(MatchingObjectDTO profile, List<MatchingObjectDTO> jobs)
    {
        var list = jobs
            .Select((j, i) => new MatchResultDTO { Id = j.Id, Score = 100.0 - i })
            .ToList();
        return Task.FromResult(list);
    }
}
