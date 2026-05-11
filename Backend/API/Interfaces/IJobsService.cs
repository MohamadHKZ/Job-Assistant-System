using API.Entities;

namespace API.Interfaces;

public interface IJobsService
{
    Task<IEnumerable<EmbeddedJobPost>> GetAllEmbeddedJobPostsAsync();
    Task<EmbeddedJobPost?> GetFullJobPostByIdAsync(Guid jobId);
}
