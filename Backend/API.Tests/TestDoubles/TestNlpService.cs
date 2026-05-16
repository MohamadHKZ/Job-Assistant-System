using API.DTOs;
using JobAssistantSystem.Backend.API.Interfaces;

namespace API.Tests.TestDoubles;

public sealed class TestNlpService : INlpService
{
    public ProfileDTO StructureProfile(string promptForProfileStructuring)
    {
        return new ProfileDTO
        {
            ProfileId = 1,
            SeekedJobTitle = "Software Engineer",
            TechnicalSkills = new List<string> { "C#" },
            FieldSkills = new List<string> { "Backend" },
            JobPositionSkills = new List<string> { "Developer" },
            SoftSkills = new List<string> { "Communication" },
            Technologies = new List<string> { ".NET" },
            Experience = "5 years",
            ReceiveNotifications = false,
        };
    }
}
