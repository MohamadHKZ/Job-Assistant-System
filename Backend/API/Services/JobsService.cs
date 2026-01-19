using API.Data;
using API.Entities;
using Microsoft.EntityFrameworkCore;

class JobsService(AppDbContext _dbContext) : IJobsService
{
    public async Task<IEnumerable<EmbeddedJobPost>> GetAllEmbeddedJobPostsAsync()
    {
        return await _dbContext.EmbeddedJobPosts.ToListAsync();
    }
    public async Task<EmbeddedJobPost?> GetFullJobPostByIdAsync(long jobId)
    {
        return await _dbContext.EmbeddedJobPosts.Include(jp => jp.NormalizedJobPost).ThenInclude(njp => njp.JobPost)
            .FirstOrDefaultAsync(jp => jp.JobPostId == jobId);
    }
}