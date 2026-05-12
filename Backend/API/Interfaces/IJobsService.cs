using API.Entities;
using Pgvector;

namespace API.Interfaces;

public interface IJobsService
{
    Task<IEnumerable<EmbeddedJobPost>> GetAllEmbeddedJobPostsAsync();
    Task<IEnumerable<EmbeddedJobPost>> GetJobsBySimilarTitleAsync(Vector profileJobTitleVector, int topN = 100);
    Task<EmbeddedJobPost?> GetFullJobPostByIdAsync(Guid jobId);
}
