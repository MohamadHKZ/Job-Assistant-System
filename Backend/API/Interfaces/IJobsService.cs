using API.Entities;
using Pgvector;

namespace API.Interfaces;

public interface IJobsService
{
    Task<IEnumerable<EmbeddedJobPost>> GetAllEmbeddedJobPostsAsync();
    Task<IEnumerable<EmbeddedJobPost>> GetJobsBySimilarTitleAsync(Vector profileJobTitleVector, int topN = 100);

    /// <summary>
    /// Keyset-paginated jobs by vector title similarity (pre-filter for ML ranking).
    /// Returns posts in vector-rank order, plus cursor values for the next page when <see cref="HasNextPage"/> is true.
    /// </summary>
    Task<(List<EmbeddedJobPost> Items, bool HasNextPage, float? NextCursorScore, Guid? NextCursorId)> GetPagedJobsAsync(
        Vector profileJobTitleVector,
        int pageSize,
        float? cursorScore,
        Guid? cursorId,
        CancellationToken cancellationToken = default);

    Task<EmbeddedJobPost?> GetFullJobPostByIdAsync(Guid jobId);
}
