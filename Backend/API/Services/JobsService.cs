using API.Data;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using Pgvector;
using Pgvector.EntityFrameworkCore;

namespace API.Services;

class JobsService(AppDbContext _dbContext) : IJobsService
{
    public async Task<IEnumerable<EmbeddedJobPost>> GetAllEmbeddedJobPostsAsync()
    {
        return await _dbContext.EmbeddedJobPosts.Include(jp => jp.NormalizedJobPost).ThenInclude(njp => njp.JobPost).ToListAsync();
    }

    public async Task<IEnumerable<EmbeddedJobPost>> GetJobsBySimilarTitleAsync(Vector profileJobTitleVector, int topN = 100)
    {
        return await _dbContext.EmbeddedJobPosts
            .Include(jp => jp.NormalizedJobPost)
                .ThenInclude(njp => njp.JobPost)
            .OrderBy(jp => jp.EmbeddedJobTitle.CosineDistance(profileJobTitleVector))
            .Take(topN)
            .ToListAsync();
    }

    public async Task<EmbeddedJobPost?> GetFullJobPostByIdAsync(Guid jobId)
    {
        return await _dbContext.EmbeddedJobPosts.Include(jp => jp.NormalizedJobPost).ThenInclude(njp => njp.JobPost)
            .FirstOrDefaultAsync(jp => jp.Id == jobId);
    }
}
