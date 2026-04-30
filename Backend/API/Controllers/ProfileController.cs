using System.Text;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using JobAssistantSystem.API.Errors;
using JobAssistantSystem.Backend.API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UglyToad.PdfPig;
namespace API.Controllers;

[Authorize]
public class ProfileController : BaseController
{
    private readonly ILogger<ProfileController> _logger;
    private readonly IEmbeddingService _embeddingService;
    private readonly INlpService _nlpService;
    private readonly IProfileService _profileService;
    private readonly AppDbContext _dbContext;

    public ProfileController(AppDbContext dbContext, IProfileService profileService, ILogger<ProfileController> logger, INlpService nlpService, IEmbeddingService embeddingService)
    {
        _dbContext = dbContext;
        _profileService = profileService;
        _logger = logger;
        _embeddingService = embeddingService;
        _nlpService = nlpService;
    }
    [HttpGet("{profileId}")]
    public async Task<IActionResult> GetProfile(int profileId)
    {
        var profile = await _dbContext.Profiles
            .Include(p => p.ProfileQualifications)
            .FirstOrDefaultAsync(p => p.ProfileId == profileId);

        var profileQualifications = profile?.ProfileQualifications;
        if (profileQualifications == null || profile == null)
            throw new ProfileNotFoundException(profileId);
        ProfileDTO profileDTO = new ProfileDTO
        {
            ProfileId = profileQualifications.ProfileId,
            SeekedJobTitle = profileQualifications.SeekedJobTitle,
            TechnicalSkills = profileQualifications.TechnicalSkills,
            FieldSkills = profileQualifications.FieldSkills,
            JobPositionSkills = profileQualifications.JobPositionSkills,
            SoftSkills = profileQualifications.SoftSkills,
            Experience = profileQualifications.Experience,
            ReceiveNotifications = profile.ReceiveNotifications
        };
        return Ok(profileDTO);
    }
    [HttpGet("{userId}/profile_id")]
    public async Task<IActionResult> GetProfileIdByUserId(int userId)
    {
        var profileId = await _profileService.GetProfileIdByUserIdAsync(userId);
        if (profileId == null)
            throw new ProfileNotFoundException(userId);
        return Ok(profileId);
    }

    [HttpPost("{userId}/save")]
    [HttpPut("{profileId}/update")]
    public async Task<IActionResult> SaveProfile(ProfileConfigDTO profileConfig, int userId, int profileId)
    {
        // Null body is already handled by [ApiController] model binding which
        // returns a 400 ValidationProblemDetails — no need to check here.
        var matchingObject = new MatchingObject
        {
            Id = profileId,
            JobTitle = profileConfig.JobTitle,
            FieldSkills = profileConfig.FieldSkills,
            SoftSkills = profileConfig.SoftSkills,
            JobPositionSkills = profileConfig.JobPositionSkills,
            TechnicalSkills = profileConfig.TechnicalSkills,
            Technologies = profileConfig.Technologies
        };

        var embeddings = await _embeddingService.EmbedJobsAsync(new[] { matchingObject });
        if (embeddings.Length == 0)
            throw new UpstreamServiceException("Embedding service returned empty result.");

        var embedding = embeddings[0];

        if (profileId == 0)
        {
            var profile = await _profileService.CreateProfileAsync(profileConfig, embedding.Embeddings, userId);
            return Ok(profile);
        }
        else
        {
            var qualifications = await _profileService.UpdateProfileAsync(profileConfig, embedding.Embeddings, profileId);
            return Ok(qualifications);
        }
    }

    // POST: /api/profile/extract-info
    // Content-Type: application/pdf
    [HttpPost("extract-info")]
    [Consumes("application/pdf")]
    public async Task<ActionResult<ProfileDTO>> ExtractInfoFromPdf()
    {
        if (Request.Body == null || !Request.Body.CanRead)
            return this.ProblemFor(
                typeSlug: "invalid-pdf-body",
                title: "Invalid or empty request body",
                statusCode: StatusCodes.Status400BadRequest,
                detail: "The request did not contain a readable PDF body.");

        await using var memoryStream = new MemoryStream();
        await Request.Body.CopyToAsync(memoryStream);
        memoryStream.Position = 0;

        // PdfPig throws if the stream isn't a valid PDF; the global handler
        // will translate that to a 500 ProblemDetails. We surface a friendlier
        // 422 only for the "no extractable text" case below.
        using var document = PdfDocument.Open(memoryStream);

        var sb = new StringBuilder();
        foreach (var page in document.GetPages())
        {
            if (!string.IsNullOrWhiteSpace(page.Text))
                sb.AppendLine(page.Text);
        }

        if (sb.Length == 0)
            return this.ProblemFor(
                typeSlug: "pdf-not-extractable",
                title: "No extractable text found",
                statusCode: StatusCodes.Status422UnprocessableEntity,
                detail: "The provided PDF did not contain any extractable text.");

        string BaseDir = Directory.GetCurrentDirectory();
        string promptPath = Path.Combine(BaseDir, "prompts", "profile_structuring_prompt.txt");
        string extraction_prompt = await System.IO.File.ReadAllTextAsync(promptPath);
        _logger.LogInformation("Extraction prompt loaded.");
        string prompt = extraction_prompt + "\n\n" + sb.ToString();
        ProfileDTO profile = _nlpService.StructureProfile(prompt);
        return Ok(profile);
    }

}
