using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

namespace API.Entities;

public class EmbeddedJobPost
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public long JobPostId { get; set; }

    [Required]
    public string SourceName { get; set; } = string.Empty;

    [Column(TypeName = "jsonb")]
    public List<SkillEmbedding> EmbeddedTechnicalSkills { get; set; } = new();

    [Column(TypeName = "jsonb")]
    public List<SkillEmbedding> EmbeddedJobPositionSkills { get; set; } = new();

    [Column(TypeName = "vector(1024)")]
    public Vector EmbeddedJobTitle { get; set; } = new(new float[1024]);

    [Column(TypeName = "jsonb")]
    public List<SkillEmbedding> EmbeddedFieldSkills { get; set; } = new();

    [Column(TypeName = "jsonb")]
    public List<SkillEmbedding> EmbeddedSoftSkills { get; set; } = new();

    [Column(TypeName = "jsonb")]
    public List<SkillEmbedding> EmbeddedTechnologies { get; set; } = new List<SkillEmbedding>();

    public NormalizedJobPost NormalizedJobPost { get; set; } = null!;
}
