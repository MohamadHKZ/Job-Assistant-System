using API.DTOs;

public interface IMatchingRankingService
{
    Task<List<MatchResultDTO>> Rank(MatchingObjectDTO profile, List<MatchingObjectDTO> jobs);
}