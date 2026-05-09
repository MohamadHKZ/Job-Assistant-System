using System.Text.Json.Serialization;

namespace API.DTOs;

public class UserDTO
{
    [JsonPropertyName("jobSeekerId")]
    public int Id { get; set; }
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Token { get; set; } = "";

    /// <summary>Primary role for UI routing (first assigned role, or "User").</summary>
    [JsonPropertyName("role")]
    public string Role { get; set; } = "User";
}
