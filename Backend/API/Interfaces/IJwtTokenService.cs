using API.Entities;

namespace JobAssistantSystem.API.Interfaces
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user, IEnumerable<string> roles);
    }
}
