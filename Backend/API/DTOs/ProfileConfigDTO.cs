using System.Text.Json;
using System.Text.Json.Serialization;

namespace API.DTOs;

public class ProfileConfigDTO
{
    [JsonPropertyName("technical_skills")]
    public List<string> TechnicalSkills { get; set; } = new();

    [JsonPropertyName("job_position_skills")]
    public List<string> JobPositionSkills { get; set; } = new();

    [JsonPropertyName("field_skills")]
    public List<string> FieldSkills { get; set; } = new();

    [JsonPropertyName("job_title")]
    public List<string> JobTitle { get; set; } = new();

    [JsonPropertyName("soft_skills")]
    public List<string> SoftSkills { get; set; } = new();

    [JsonPropertyName("experience")]
    public string Experience { get; set; } = "";

    [JsonPropertyName("receive_notifications")]
    public bool ReceiveNotifications { get; set; } = false;

    public override string ToString() =>
        JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
}