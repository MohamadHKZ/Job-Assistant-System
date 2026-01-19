using System.Text.Json;
using System.Text.Json.Serialization;

namespace API.Entities;

public class MatchingObject
{
    [JsonPropertyName("id")]
    public int Id { get; set; }

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

    public override string ToString() =>
        JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
}