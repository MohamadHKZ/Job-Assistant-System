using System.Text.Json.Serialization;

namespace API.Entities;

public class SkillEmbedding
{
    [JsonPropertyName("skill")]
    public string Skill { get; set; } = string.Empty;

    [JsonPropertyName("vector")]
    public List<double> Vector { get; set; } = new();
}

public class EmbeddingCategories
{
    [JsonPropertyName("technical_skills")]
    public List<SkillEmbedding> TechnicalSkills { get; set; } = new();

    [JsonPropertyName("job_position_skills")]
    public List<SkillEmbedding> JobPositionSkills { get; set; } = new();

    [JsonPropertyName("field_skills")]
    public List<SkillEmbedding> FieldSkills { get; set; } = new();

    [JsonPropertyName("job_title")]
    public List<SkillEmbedding> JobTitle { get; set; } = new();

    [JsonPropertyName("soft_skills")]
    public List<SkillEmbedding> SoftSkills { get; set; } = new();
}

public class EmbeddingEntity
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("embeddings")]
    public EmbeddingCategories Embeddings { get; set; } = new();
}