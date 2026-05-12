using API.Data;
using API.DTOs;
using API.Entities;
using JobAssistantSystem.API.Errors;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Pgvector;

class ProfileService(AppDbContext _dbContext, ILogger<ProfileService> _logger) : IProfileService
{
    private static Vector JobTitleEmbeddingsToVector(List<SkillEmbedding> jobTitle)
    {
        var floats = new float[1024];
        var doubles = jobTitle.FirstOrDefault()?.Vector;
        if (doubles is { Count: > 0 })
        {
            var n = Math.Min(doubles.Count, 1024);
            for (var i = 0; i < n; i++)
                floats[i] = (float)doubles[i];
        }

        return new Vector(floats);
    }

    public async Task<Profile> CreateProfileAsync(ProfileConfigDTO profileConfig, EmbeddingCategories embedding, int userId)
    {
        Profile profile = new Profile();
        ProfileQualifications qualifications = new ProfileQualifications();
        ProfileConfigToProfileAndQualifications(profileConfig, profile, qualifications);
        profile.UserId = userId;
        await _dbContext.Profiles.AddAsync(profile);
        await _dbContext.ProfilesQualifications.AddAsync(qualifications);
        EmbeddedProfile embeddedProfile = new EmbeddedProfile
        {
            Profile = profile,
            EmbeddedTechnicalSkills = embedding.TechnicalSkills,
            EmbeddedFieldSkills = embedding.FieldSkills,
            EmbeddedSoftSkills = embedding.SoftSkills,
            EmbeddedJobPositionSkills = embedding.JobPositionSkills,
            EmbeddedTechnologies = embedding.Technologies,
            EmbeddedJobTitle = JobTitleEmbeddingsToVector(embedding.JobTitle)
        };
        await _dbContext.EmbeddedProfiles.AddAsync(embeddedProfile);
        await _dbContext.SaveChangesAsync();
        return profile;
    }

    public async Task<EmbeddedProfile?> GetEmbeddedProfileByIdAsync(int profileId)
    {
        return await _dbContext.EmbeddedProfiles
            .FirstOrDefaultAsync(e => e.ProfileId == profileId);
    }

    public async Task<Profile?> GetProfileByIdAsync(int profileId)
    {
        return await _dbContext.Profiles.FirstOrDefaultAsync(p => p.ProfileId == profileId);
    }

    public Task<int?> GetProfileIdByUserIdAsync(int userId)
    {
        var profileId = _dbContext.Profiles
            .Where(p => p.UserId == userId)
            .Select(p => (int?)p.ProfileId)
            .FirstOrDefaultAsync();
        return profileId;
    }

    public async Task<ProfileQualifications?> GetProfileQualificationsByIdAsync(int profileId)
    {
        return await _dbContext.ProfilesQualifications.FirstOrDefaultAsync(p => p.ProfileId == profileId);
    }

    public async Task<ProfileQualifications> UpdateProfileAsync(ProfileConfigDTO profileConfig, EmbeddingCategories embedding, int profileId)
    {
        var profile = await _dbContext.Profiles
            .Include(p => p.ProfileQualifications)
            .FirstOrDefaultAsync(p => p.ProfileId == profileId);
        if (profile == null)
        {
            _logger.LogWarning("UpdateProfile: profile {ProfileId} not found", profileId);
            throw new NotFoundException("profile", profileId);
        }

        var qualifications = profile.ProfileQualifications;
        if (qualifications == null)
        {
            _logger.LogWarning("UpdateProfile: qualifications missing for profile {ProfileId}", profileId);
            throw new NotFoundException("profile", profileId);
        }
        ProfileConfigToProfileAndQualifications(profileConfig, profile, qualifications);
        var embeddedProfile = await _dbContext.EmbeddedProfiles
            .FirstOrDefaultAsync(e => e.ProfileId == profileId);
        if (embeddedProfile != null)
        {
            embeddedProfile.EmbeddedTechnicalSkills = embedding.TechnicalSkills;
            embeddedProfile.EmbeddedFieldSkills = embedding.FieldSkills;
            embeddedProfile.EmbeddedSoftSkills = embedding.SoftSkills;
            embeddedProfile.EmbeddedJobPositionSkills = embedding.JobPositionSkills;
            embeddedProfile.EmbeddedJobTitle = JobTitleEmbeddingsToVector(embedding.JobTitle);
            embeddedProfile.EmbeddedTechnologies = embedding.Technologies;
        }
        await _dbContext.SaveChangesAsync();
        return qualifications;
    }
    private void ProfileConfigToProfileAndQualifications(ProfileConfigDTO profileConfig, Profile profile, ProfileQualifications qualifications)
    {
        profile.ReceiveNotifications = profileConfig.ReceiveNotifications;
        profile.IsActive = true;

        qualifications.SeekedJobTitle = profileConfig.JobTitle;
        qualifications.TechnicalSkills = profileConfig.TechnicalSkills;
        qualifications.FieldSkills = profileConfig.FieldSkills;
        qualifications.SoftSkills = profileConfig.SoftSkills;
        qualifications.JobPositionSkills = profileConfig.JobPositionSkills;
        qualifications.Experience = profileConfig.Experience;
        qualifications.Technologies = profileConfig.Technologies;
        qualifications.Profile = profile;
    }
}
