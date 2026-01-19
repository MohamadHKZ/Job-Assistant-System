using System.ComponentModel.DataAnnotations;

namespace Backend.API.DTOs
{
    public class JobPostDTO
    {
        public long Id { get; set; }
        public string? JobTitle { get; set; }
        public string? CompanyName { get; set; }
        public string? JobDescription { get; set; }
        public string? Location { get; set; }
        public string? ExperienceLevel { get; set; }
        public string? JobType { get; set; }
        public List<string>? TechnicalSkills { get; set; }
        [Url]
        public string? Url { get; set; }
    }
}