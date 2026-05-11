using API.DTOs;

namespace JobAssistantSystem.Backend.API.Interfaces
{
    public interface INlpService
    {
        ProfileDTO StructureProfile(string promptForProfileStructuring);
    }
}
