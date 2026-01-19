using System.Text;
using System.Text.Json;
using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
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
    private readonly INlpEmbeddingService _nlpEmbeddingService;
    private readonly INlpService _nlpService;
    private readonly IProfileService _profileService;
    private readonly AppDbContext _dbContext;

    public ProfileController(AppDbContext dbContext, IProfileService profileService, ILogger<ProfileController> logger, INlpService nlpService, INlpEmbeddingService nlpEmbeddingService)
    {
        _dbContext = dbContext;
        _profileService = profileService;
        _logger = logger;
        _nlpEmbeddingService = nlpEmbeddingService;
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
            return NotFound("Profile not found");
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
            return NotFound("Profile not found");
        return Ok(profileId);
    }

    [HttpPost("{userId}/save")]
    [HttpPut("{profileId}/update")]
    public async Task<IActionResult> SaveProfile(ProfileConfigDTO profileConfig, int userId, int profileId)
    {
        if (profileConfig is null) return BadRequest("ProfileInfo is required.");
        var json = JsonSerializer.Serialize(profileConfig);
        string BaseDir = Directory.GetCurrentDirectory();
        string promptPath = Path.Combine(BaseDir, "prompts", "matching_object_prompt.txt");
        var prompt = await System.IO.File.ReadAllTextAsync(promptPath);
        var requestString = prompt + "\n\n" + json;
        var response = await _nlpEmbeddingService.StructureAndEmbed(requestString);
        var refinedProfileConfig = response[0].MatchingObject;
        var embedding = response[0].Embedding;
        profileConfig.JobTitle = refinedProfileConfig.JobTitle;
        profileConfig.FieldSkills = refinedProfileConfig.FieldSkills;
        profileConfig.SoftSkills = refinedProfileConfig.SoftSkills;
        profileConfig.JobPositionSkills = refinedProfileConfig.JobPositionSkills;
        profileConfig.TechnicalSkills = refinedProfileConfig.TechnicalSkills;
        Console.WriteLine(embedding.Embeddings.JobTitle[0].Vector.Count);
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
            return BadRequest("Invalid or empty request body.");

        try
        {
            await using var memoryStream = new MemoryStream();
            await Request.Body.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            using var document = PdfDocument.Open(memoryStream);

            var sb = new StringBuilder();
            foreach (var page in document.GetPages())
            {
                if (!string.IsNullOrWhiteSpace(page.Text))
                    sb.AppendLine(page.Text);
            }

            if (sb.Length == 0)
                return UnprocessableEntity("No extractable text found.");

            string BaseDir = Directory.GetCurrentDirectory();
            string promptPath = Path.Combine(BaseDir, "prompts", "profile_structuring_prompt.txt");
            string extraction_prompt = await System.IO.File.ReadAllTextAsync(promptPath);
            Console.WriteLine("Extraction Prompt Loaded.");
            string prompt = extraction_prompt + "\n\n" + sb.ToString();
            ProfileDTO profile = _nlpService.StructureProfile(prompt);
            return Ok(profile);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            return UnprocessableEntity("Could not read/extract text from the provided PDF.");
        }
    }

}
