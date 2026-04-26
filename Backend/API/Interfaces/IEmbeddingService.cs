using API.Entities;

namespace API.Interfaces;

public interface IEmbeddingService
{
    Task<EmbeddingEntity[]> EmbedJobsAsync(IEnumerable<MatchingObject> jobs, CancellationToken cancellationToken = default);
}
