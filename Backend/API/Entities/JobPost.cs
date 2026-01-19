using System.ComponentModel.DataAnnotations;

namespace API.Entities;

public class JobPost
{
    [Key]
    public long JobPostId { get; set; }
    [Key]
    public string SourceName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public string JobDescription { get; set; } = string.Empty;
    public string JobType { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string ExperienceLevel { get; set; } = string.Empty;
    public DateTime PostedDate { get; set; } = DateTime.UtcNow;
    [Url]
    public string Url { get; set; } = string.Empty;
    public string CompanyName { get; set; } = string.Empty;
    public JobSource JobSource { get; set; } = null!;
}
