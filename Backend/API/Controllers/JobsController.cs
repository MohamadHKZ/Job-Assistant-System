using API.Controllers;
using API.DTOs;
using API.Entities;
using Backend.API.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace Job_Assistant_System.API.Controllers
{
    [Authorize]
    public class JobsController(IJobsService _jobsService, IProfileService _profileService, IMatchingRankingService _matchingRankingService, ILogger<JobsController> _logger) : BaseController
    {
        [HttpGet("{profileId}")]
        public async Task<ActionResult<IEnumerable<JobPostDTO>>> GetJobs(int profileId)
        {
            var embeddedProfile = await _profileService.GetEmbeddedProfileByIdAsync(profileId);
            var profileQualifications = await _profileService.GetProfileQualificationsByIdAsync(profileId);
            var embeddedJobPosts = await _jobsService.GetAllEmbeddedJobPostsAsync();
#pragma warning disable CS8601 // Possible null reference assignment.c
            var profileEntity = new MatchingObjectDTO
            {
                Id = embeddedProfile.ProfileId,
                Title = profileQualifications?.SeekedJobTitle?.FirstOrDefault() ?? string.Empty,
                Experience = profileQualifications?.Experience ?? string.Empty,
                Techonologies = profileQualifications?.Technologies ?? new(),
                Embeddings = new EmbeddingCategories
                {
                    TechnicalSkills = embeddedProfile?.EmbeddedTechnicalSkills,
                    JobPositionSkills = embeddedProfile?.EmbeddedJobPositionSkills,
                    FieldSkills = embeddedProfile?.EmbeddedFieldSkills,
                    JobTitle = embeddedProfile?.EmbeddedJobTitle,
                    SoftSkills = embeddedProfile?.EmbeddedSoftSkills,
                    Technologies = embeddedProfile?.EmbeddedTechnologies
                }
            };
#pragma warning restore CS8601 // Possible null reference assignment.

            var jobPostsEntity = embeddedJobPosts.Select(jp => new MatchingObjectDTO
            {
                Id = jp.JobPostId,
                Title = jp.NormalizedJobPost?.JobPost?.JobTitle ?? string.Empty,
                Experience = jp.NormalizedJobPost?.ExperienceLevelRefined ?? string.Empty,
                Techonologies = jp.NormalizedJobPost?.Technologies ?? new(),
                Embeddings = new EmbeddingCategories
                {
                    TechnicalSkills = jp.EmbeddedTechnicalSkills,
                    JobPositionSkills = jp.EmbeddedJobPositionSkills,
                    FieldSkills = jp.EmbeddedFieldSkills,
                    JobTitle = jp.EmbeddedJobTitle,
                    SoftSkills = jp.EmbeddedSoftSkills,
                    Technologies = jp.EmbeddedTechnologies
                }
            }).ToList();

            LogMatchingObjectsBeforeRanking(profileId, profileEntity, jobPostsEntity);
            var rankedJobs = await _matchingRankingService.Rank(profileEntity, jobPostsEntity);
            LogRankedJobs(profileId, rankedJobs);

            var rankedJobIds = rankedJobs
                .Where(rj => rj.Score > 60)
                .OrderByDescending(rj => rj.Score)
                .Select(rj => rj.Id)
                .ToList();

            var embeddedJobPostsById = embeddedJobPosts.ToLookup(jp => jp.JobPostId);
            var jobPostsDTO = new List<JobPostDTO>();
            foreach (var rankedJobId in rankedJobIds)
            {
                EmbeddedJobPost? fullJobPost = embeddedJobPostsById[rankedJobId].FirstOrDefault();
                if (fullJobPost is null)
                {
                    Console.WriteLine($"Warning: Embedded job post with ID {rankedJobId} not found. Skipping this job.");
                    continue;
                }

                NormalizedJobPost? normalizedJobPost = fullJobPost.NormalizedJobPost;
                JobPost? jobPost = normalizedJobPost?.JobPost;

                jobPostsDTO.Add(new JobPostDTO
                {
                    Id = rankedJobId,
                    JobTitle = jobPost?.JobTitle,
                    CompanyName = jobPost?.CompanyName,
                    Location = jobPost?.Location,
                    JobType = jobPost?.JobType,
                    JobDescription = jobPost?.JobDescription,
                    TechnicalSkills = normalizedJobPost?.RequiredTechnicalSkills,
                    Url = jobPost?.Url,
                    ExperienceLevel = normalizedJobPost?.ExperienceLevelRefined
                });
            }
            return Ok(jobPostsDTO);
        }

        private void LogMatchingObjectsBeforeRanking(int profileId, MatchingObjectDTO profileEntity, IEnumerable<MatchingObjectDTO> jobPostsEntity)
        {
            try
            {
                _logger.LogInformation(
                    "Sending matching payload for profile {ProfileId}. Profile object: {ProfileObjectWithoutEmbeddings}",
                    profileId,
                    JsonSerializer.Serialize(GetMatchingObjectWithoutEmbeddings(profileEntity))
                );

                int iteration = 0;
                foreach (var jobEntity in jobPostsEntity)
                {
                    iteration++;
                    _logger.LogInformation(
                        "Matching payload item {Iteration} for profile {ProfileId}. Job object: {JobObjectWithoutEmbeddings}",
                        iteration,
                        profileId,
                        JsonSerializer.Serialize(GetMatchingObjectWithoutEmbeddings(jobEntity))
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log matching objects before ranking for profile {ProfileId}", profileId);
            }
        }

        private static object GetMatchingObjectWithoutEmbeddings(MatchingObjectDTO entity)
        {
            return new
            {
                entity.Id,
                entity.Title,
                entity.Experience,
                entity.Techonologies
            };
        }

        private void LogRankedJobs(int profileId, IEnumerable<MatchResultDTO> rankedJobs)
        {
            try
            {
                var orderedRankedJobs = rankedJobs.OrderByDescending(job => job.Score).ToList();

                _logger.LogInformation("Ranked jobs generated for profile {ProfileId}. Total results: {TotalResults}", profileId, orderedRankedJobs.Count);

                foreach (var job in orderedRankedJobs)
                {
                    _logger.LogInformation("Profile {ProfileId} ranked job -> JobId: {JobId}, Score: {Score}", profileId, job.Id, job.Score);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to log ranked jobs output for profile {ProfileId}", profileId);
            }
        }

        [HttpGet()]
        public async Task<ActionResult<JobPostDTO>> GetJobById(int id)
        {
            // TODO: Implement get job by id logic
            return Ok(new JobPostDTO());
        }
    }
}