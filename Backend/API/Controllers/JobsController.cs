using API.Controllers;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Backend.API.DTOs;
using JobAssistantSystem.API.Errors;
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
        public async Task<ActionResult<JobsPageDTO>> GetJobs(
            int profileId,
            [FromQuery] float? cursorScore = null,
            [FromQuery] Guid? cursorId = null,
            CancellationToken cancellationToken = default)
        {
            var profile = await _profileService.GetProfileByIdAsync(profileId);
            if (profile is null)
                throw new NotFoundException("profile", profileId);

            var embeddedProfile = await _profileService.GetEmbeddedProfileByIdAsync(profileId);
            var profileQualifications = await _profileService.GetProfileQualificationsByIdAsync(profileId);

            const int pageSize = 5;
            var (embeddedJobPostsList, hasNextPage, nextCursorScore, nextCursorId) =
                await _jobsService.GetPagedJobsAsync(
                    embeddedProfile!.EmbeddedJobTitle,
                    pageSize,
                    cursorScore,
                    cursorId,
                    cancellationToken);

            var embeddedJobPosts = embeddedJobPostsList;
#pragma warning disable CS8601 // Possible null reference assignment.c
            var profileEntity = new MatchingObjectDTO
            {
                Id = embeddedProfile!.ProfileId.ToString(),
                Title = profileQualifications?.SeekedJobTitle ?? string.Empty,
                Experience = profileQualifications?.Experience ?? string.Empty,
                Techonologies = profileQualifications?.Technologies ?? new(),
                Embeddings = new EmbeddingCategories
                {
                    JobPositionSkills = embeddedProfile?.EmbeddedJobPositionSkills,
                    Technologies = embeddedProfile?.EmbeddedTechnologies
                }
            };
#pragma warning restore CS8601 // Possible null reference assignment.

            var jobPostsEntity = embeddedJobPosts.Select(jp => new MatchingObjectDTO
            {
                Id = jp.Id.ToString(),
                Title = jp.NormalizedJobPost?.JobPost?.JobTitle ?? string.Empty,
                Experience = jp.NormalizedJobPost?.ExperienceLevelRefined ?? string.Empty,
                Techonologies = jp.NormalizedJobPost?.Technologies ?? new(),
                Embeddings = new EmbeddingCategories
                {
                    JobPositionSkills = jp.EmbeddedJobPositionSkills,
                    Technologies = jp.EmbeddedTechnologies
                }
            }).ToList();

            LogMatchingObjectsBeforeRanking(profileId, profileEntity, jobPostsEntity);
            var rankedJobs = await _matchingRankingService.Rank(profileEntity, jobPostsEntity);
            LogRankedJobs(profileId, rankedJobs);

            var rankedJobIds = rankedJobs
                .OrderByDescending(rj => rj.Score)
                .Select(rj => rj.Id)
                .ToList();

            var embeddedJobPostsById = embeddedJobPosts.ToLookup(jp => jp.Id.ToString());
            var jobPostsDTO = new List<JobPostDTO>();
            var scoreById = rankedJobs.ToDictionary(rj => rj.Id, rj => rj.Score);

            foreach (var rankedJobId in rankedJobIds)
            {
                EmbeddedJobPost? fullJobPost = embeddedJobPostsById[rankedJobId].FirstOrDefault();
                if (fullJobPost is null)
                {
                    _logger.LogWarning("Embedded job post {JobId} not found in lookup; skipping", rankedJobId);
                    continue;
                }

                if (!Guid.TryParse(rankedJobId, out var jobGuid))
                {
                    _logger.LogWarning("Invalid job id from matcher: {JobId}", rankedJobId);
                    continue;
                }

                NormalizedJobPost? normalizedJobPost = fullJobPost.NormalizedJobPost;
                JobPost? jobPost = normalizedJobPost?.JobPost;

                scoreById.TryGetValue(rankedJobId, out var mlScore);

                jobPostsDTO.Add(new JobPostDTO
                {
                    Id = jobGuid,
                    JobTitle = jobPost?.JobTitle,
                    CompanyName = jobPost?.CompanyName,
                    Location = jobPost?.Location,
                    JobType = jobPost?.JobType,
                    JobDescription = jobPost?.JobDescription,
                    TechnicalSkills = normalizedJobPost?.RequiredTechnicalSkills,
                    Url = jobPost?.Url,
                    ExperienceLevel = normalizedJobPost?.ExperienceLevelRefined,
                    Score = (float)mlScore
                });
            }

            return Ok(new JobsPageDTO
            {
                Jobs = jobPostsDTO,
                HasNextPage = hasNextPage,
                NextCursorScore = nextCursorScore,
                NextCursorId = nextCursorId
            });
        }

        private void LogMatchingObjectsBeforeRanking(int profileId, MatchingObjectDTO profileEntity, List<MatchingObjectDTO> jobPostsList)
        {
            _logger.LogInformation(
                "Matching payload prepared for profile {ProfileId}: {JobCount} jobs, profile title={ProfileTitle}",
                profileId, jobPostsList.Count, profileEntity.Title);

            if (!_logger.IsEnabled(LogLevel.Debug))
                return;

            _logger.LogDebug(
                "Profile payload (no embeddings): {Payload}",
                JsonSerializer.Serialize(GetMatchingObjectWithoutEmbeddings(profileEntity)));

            var iteration = 0;
            foreach (var jobEntity in jobPostsList)
            {
                iteration++;
                _logger.LogDebug(
                    "Job payload [{Iteration}] for profile {ProfileId}: {Payload}",
                    iteration,
                    profileId,
                    JsonSerializer.Serialize(GetMatchingObjectWithoutEmbeddings(jobEntity)));
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
            var orderedRankedJobs = rankedJobs.OrderByDescending(job => job.Score).ToList();

            _logger.LogInformation(
                "Ranked jobs generated for profile {ProfileId}. Total results: {TotalResults}",
                profileId, orderedRankedJobs.Count);

            if (!_logger.IsEnabled(LogLevel.Debug))
                return;

            foreach (var job in orderedRankedJobs)
            {
                _logger.LogDebug(
                    "Profile {ProfileId} ranked job -> JobId: {JobId}, Score: {Score}",
                    profileId, job.Id, job.Score);
            }
        }

        [HttpGet]
        public async Task<ActionResult<JobPostDTO>> GetJobById([FromQuery] Guid jobId)
        {
            var job = await _jobsService.GetFullJobPostByIdAsync(jobId);
            if (job is null)
                throw new NotFoundException("job", jobId.ToString());

            // TODO: Implement get job by id logic
            return Ok(new JobPostDTO());
        }
    }
}
