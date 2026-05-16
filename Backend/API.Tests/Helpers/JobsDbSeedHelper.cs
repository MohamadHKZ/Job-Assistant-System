using API.Data;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using Pgvector;

namespace API.Tests.Helpers;

public static class JobsDbSeedHelper
{
    /// <summary>Creates embedded job posts whose title vectors match TestEmbeddingService (0.01 in all dimensions) so similarity score stays above 0.6.
    /// </summary>
    public static async Task SeedEmbeddedJobPostsAsync(AppDbContext db, string sourceName, int count)
    {
        if (!await db.JobSources.AnyAsync(js => js.SourceName == sourceName))
        {
            db.JobSources.Add(new JobSource { SourceName = sourceName, IsActive = true });
            await db.SaveChangesAsync();
        }

        var vec = new Vector(Enumerable.Repeat(0.01f, 1024).ToArray());

        for (var i = 0; i < count; i++)
        {
            var id = Guid.NewGuid();
            var jobPostId = 10_000_000L + i + Random.Shared.Next(0, 1_000_000);

            db.JobPosts.Add(new JobPost
            {
                Id = id,
                JobPostId = jobPostId,
                SourceName = sourceName,
                JobTitle = $"Seeded Job {i}",
                JobDescription = "Description",
                JobType = "FullTime",
                Location = "Remote",
                Url = "https://example.com/job",
                CompanyName = "TestCo",
            });

            db.NormalizedJobPosts.Add(new NormalizedJobPost
            {
                Id = id,
                JobPostId = jobPostId,
                SourceName = sourceName,
                ExperienceLevelRefined = "Mid",
            });

            db.EmbeddedJobPosts.Add(new EmbeddedJobPost
            {
                Id = id,
                JobPostId = jobPostId,
                SourceName = sourceName,
                EmbeddedJobTitle = vec,
            });
        }

        await db.SaveChangesAsync();
    }
}
