using API.Data;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

class JobsService(AppDbContext _dbContext) : IJobsService
{
    public async Task<IEnumerable<EmbeddedJobPost>> GetAllEmbeddedJobPostsAsync()
    {
        return await _dbContext.EmbeddedJobPosts.Include(jp => jp.NormalizedJobPost).ThenInclude(njp => njp.JobPost).ToListAsync();
    }

    public async Task<EmbeddedJobPost?> GetFullJobPostByIdAsync(Guid jobId)
    {
        return await _dbContext.EmbeddedJobPosts.Include(jp => jp.NormalizedJobPost).ThenInclude(njp => njp.JobPost)
            .FirstOrDefaultAsync(jp => jp.Id == jobId);
    }
}
