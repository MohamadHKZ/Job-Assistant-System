using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities;

public class EmbeddedJobPost
{
    [Key]
    public long JobPostId { get; set; }
    [Key]
    public string SourceName { get; set; } = string.Empty;

    [Column(TypeName = "jsonb")]
    public List<SkillEmbedding> EmbeddedTechnicalSkills { get; set; } = new();

    [Column(TypeName = "jsonb")]
    public List<SkillEmbedding> EmbeddedJobPositionSkills { get; set; } = new();

    [Column(TypeName = "jsonb")]
    public List<SkillEmbedding> EmbeddedJobTitle { get; set; } = new();

    [Column(TypeName = "jsonb")]
    public List<SkillEmbedding> EmbeddedFieldSkills { get; set; } = new();

    [Column(TypeName = "jsonb")]
    public List<SkillEmbedding> EmbeddedSoftSkills { get; set; } = new();
    public NormalizedJobPost NormalizedJobPost { get; set; } = null!;
}
