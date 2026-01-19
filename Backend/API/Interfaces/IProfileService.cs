using API.DTOs;
using API.Entities;

public interface IProfileService
{
    Task<Profile> CreateProfileAsync(ProfileConfigDTO profileConfig, EmbeddingCategories embedding, int userId);
    Task<Profile?> GetProfileByIdAsync(int profileId);
    Task<ProfileQualifications?> GetProfileQualificationsByIdAsync(int profileId);
    Task<int?> GetProfileIdByUserIdAsync(int userId);
    Task<ProfileQualifications> UpdateProfileAsync(ProfileConfigDTO profileConfig, EmbeddingCategories embedding, int profileId);
    Task<EmbeddedProfile> GetEmbeddedProfileByIdAsync(int profileId);
}