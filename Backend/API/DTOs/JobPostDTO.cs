using System.ComponentModel.DataAnnotations;

namespace Backend.API.DTOs
{
    public class JobPostDTO
    {
        public Guid Id { get; set; }
        public string? JobTitle { get; set; }
        public string? CompanyName { get; set; }
        public string? JobDescription { get; set; }
        public string? Location { get; set; }
        public string? ExperienceLevel { get; set; }
        public string? JobType { get; set; }
        public List<string>? TechnicalSkills { get; set; }
        [Url]
        public string? Url { get; set; }

        /// <summary>ML ranking score (0–100) from MatchingRankingService.</summary>
        public float Score { get; set; }
    }

    public class JobsPageDTO
    {
        public List<JobPostDTO> Jobs { get; set; } = new();
        public bool HasNextPage { get; set; }
        public float? NextCursorScore { get; set; }
        public Guid? NextCursorId { get; set; }
    }
}
