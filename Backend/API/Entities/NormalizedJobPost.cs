using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities;

public class NormalizedJobPost
{
    [Key]
    public long JobPostId { get; set; }
    [Key]
    public string SourceName { get; set; } = string.Empty;
    public string ExperienceLevelRefined { get; set; } = string.Empty;
    [Column(TypeName = "jsonb")]
    public List<string> JobTitleRefined { get; set; } = new();

    [Column(TypeName = "jsonb")]
    public List<string> RequiredFieldSkills { get; set; } = new();

    [Column(TypeName = "jsonb")]
    public List<string> RequiredJobPositionSkills { get; set; } = new();

    [Column(TypeName = "jsonb")]
    public List<string> RequiredTechnicalSkills { get; set; } = new();

    [Column(TypeName = "jsonb")]
    public List<string> RequiredSoftSkills { get; set; } = new();
    public JobPost JobPost { get; set; } = null!;
}
