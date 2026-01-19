using API.Entities;

public interface IMatchingRankingService
{
    Task<List<MatchResultDTO>> Rank(EmbeddingEntity ProfileEmbedding, List<EmbeddingEntity> JobEmbeddings);
}