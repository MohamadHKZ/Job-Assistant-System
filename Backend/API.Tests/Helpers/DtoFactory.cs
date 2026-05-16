using API.DTOs;
using Bogus;

namespace API.Tests.Helpers;

public static class DtoFactory
{
    private static readonly Faker Faker = new();

    public static RegisterPayload Register(string? email = null, string? password = null) =>
        new(
            Faker.Name.FirstName(),
            Faker.Name.LastName(),
            email ?? Faker.Internet.Email(),
            password ?? $"Pw!{Faker.Random.AlphaNumeric(12)}");

    public static ProfileConfigPayload ProfileConfig(string jobTitle = "Software Engineer") =>
        new(
            new List<string> { "C#" },
            new List<string> { "Backend" },
            new List<string> { "Algorithms" },
            jobTitle,
            new List<string> { "Teamwork" },
            new List<string> { ".NET" },
            "3 years",
            false);

    public record RegisterPayload(string FirstName, string LastName, string Email, string Password);

    public record ProfileConfigPayload(
        List<string> TechnicalSkills,
        List<string> JobPositionSkills,
        List<string> FieldSkills,
        string JobTitle,
        List<string> SoftSkills,
        List<string> Technologies,
        string Experience,
        bool ReceiveNotifications);

    public static object ToJsonBody(ProfileConfigPayload p) => new
    {
        technical_skills = p.TechnicalSkills,
        job_position_skills = p.JobPositionSkills,
        field_skills = p.FieldSkills,
        job_title = p.JobTitle,
        soft_skills = p.SoftSkills,
        technologies = p.Technologies,
        experience = p.Experience,
        receive_notifications = p.ReceiveNotifications,
    };
}
