using API.DTOs;
using API.Entities;

public static class AppUserExtensions
{
    public static UserDTO ToUserDTO(this User user, string token, IEnumerable<string>? roles = null)
    {
        var roleList = roles?
            .Where(static r => !string.IsNullOrWhiteSpace(r))
            .Select(static r => r.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList() ?? [];

        var primaryRole = roleList.FirstOrDefault() ?? "User";

        return new UserDTO
        {
            Id = user.UserId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Token = token,
            Role = primaryRole,
        };
    }
}
