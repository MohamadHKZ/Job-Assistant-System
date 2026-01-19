using API.Entities;

public interface IJobsService
{
    Task<IEnumerable<EmbeddedJobPost>> GetAllEmbeddedJobPostsAsync();
    Task<EmbeddedJobPost?> GetFullJobPostByIdAsync(long jobId);
}