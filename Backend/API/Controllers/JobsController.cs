using API.Entities;
using Backend.API.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Job_Assistant_System.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController(IJobsService _jobsService, IProfileService _profileService, IMatchingRankingService _matchingRankingService) : ControllerBase
    {
        [HttpGet("{profileId}")]
        public async Task<ActionResult<IEnumerable<JobPostDTO>>> GetJobs(int profileId)
        {
            var embeddedProfile = await _profileService.GetEmbeddedProfileByIdAsync(profileId);
            var EmbeddedJobPosts = await _jobsService.GetAllEmbeddedJobPostsAsync();
#pragma warning disable CS8601 // Possible null reference assignment.
            var profileEntity = new EmbeddingEntity
            {
                Id = embeddedProfile.ProfileId,
                Embeddings = new EmbeddingCategories
                {
                    TechnicalSkills = embeddedProfile?.EmbeddedTechnicalSkills,
                    JobPositionSkills = embeddedProfile?.EmbeddedJobPositionSkills,
                    FieldSkills = embeddedProfile?.EmbeddedFieldSkills,
                    JobTitle = embeddedProfile?.EmbeddedJobTitle,
                    SoftSkills = embeddedProfile?.EmbeddedSoftSkills
                }
            };
#pragma warning restore CS8601 // Possible null reference assignment.

            var jobPostsEntity = EmbeddedJobPosts.Select(jp => new EmbeddingEntity
            {
                Id = jp.JobPostId,
                Embeddings = new EmbeddingCategories
                {
                    TechnicalSkills = jp.EmbeddedTechnicalSkills,
                    JobPositionSkills = jp.EmbeddedJobPositionSkills,
                    FieldSkills = jp.EmbeddedFieldSkills,
                    JobTitle = jp.EmbeddedJobTitle,
                    SoftSkills = jp.EmbeddedSoftSkills
                }
            }).ToList();
            var rankedJobs = await _matchingRankingService.Rank(profileEntity, jobPostsEntity);

#pragma warning disable CS8602 // Dereference of a possibly null reference.

            JobPostDTO[] jobPostsDTO = await Task.WhenAll(EmbeddedJobPosts.Where(jp => rankedJobs.Find(rj => rj.Id == jp.JobPostId).Score > 60).Select(async jp =>
            {
                EmbeddedJobPost? fullJobPost = _jobsService.GetFullJobPostByIdAsync(jp.JobPostId).Result;//require editing cuz the primary key is (id,sourceName) but its guranteed to work for now
                NormalizedJobPost? normalizedJobPost = fullJobPost?.NormalizedJobPost;
                JobPost? jobPost = normalizedJobPost?.JobPost;
                var jobPostDto = new JobPostDTO
                {
                    Id = jp.JobPostId,
                    JobTitle = jobPost?.JobTitle,
                    CompanyName = jobPost?.CompanyName,
                    Location = jobPost?.Location,
                    JobType = jobPost?.JobType,
                    JobDescription = jobPost?.JobDescription,
                    TechnicalSkills = normalizedJobPost?.RequiredTechnicalSkills,
                    Url = jobPost?.Url,
                    ExperienceLevel = normalizedJobPost?.ExperienceLevelRefined
                };
                return jobPostDto;
            }));
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            return Ok(jobPostsDTO);
        }

        [HttpGet()]
        public async Task<ActionResult<JobPostDTO>> GetJobById(int id)
        {
            // TODO: Implement get job by id logic
            return Ok(new JobPostDTO());
        }
    }
}