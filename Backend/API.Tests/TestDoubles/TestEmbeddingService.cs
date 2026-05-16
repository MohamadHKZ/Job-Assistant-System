using API.Entities;
using API.Interfaces;

namespace API.Tests.TestDoubles;

/// <summary>Deterministic embeddings for integration tests (no real embedding service).
/// </summary>
public sealed class TestEmbeddingService : IEmbeddingService
{
    private static EmbeddingCategories BuildCategories()
    {
        var vec = Enumerable.Repeat(0.01, 1024).Select(d => (double)d).ToList();
        var skill = new SkillEmbedding { Skill = "test", Vector = vec };
        return new EmbeddingCategories
        {
            JobTitle = new List<SkillEmbedding> { skill },
            TechnicalSkills = new List<SkillEmbedding> { skill },
            FieldSkills = new List<SkillEmbedding> { skill },
            JobPositionSkills = new List<SkillEmbedding> { skill },
            SoftSkills = new List<SkillEmbedding> { skill },
            Technologies = new List<SkillEmbedding> { skill },
        };
    }

    public Task<EmbeddingEntity[]> EmbedJobsAsync(IEnumerable<MatchingObject> jobs, CancellationToken cancellationToken = default)
    {
        var arr = jobs?.ToArray() ?? Array.Empty<MatchingObject>();
        var cats = BuildCategories();
        var result = arr.Select((j, i) => new EmbeddingEntity { Id = j.Id != 0 ? j.Id : i + 1, Embeddings = cats }).ToArray();
        return Task.FromResult(result);
    }
}
