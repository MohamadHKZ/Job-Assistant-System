using System.Text.Json;using System.Text.Json.Serialization;

namespace API.DTOs;

/// <summary>Accepts JSON string or JSON array of strings (legacy); always exposes a single string.</summary>
public sealed class JobTitleStringJsonConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.String)
            return reader.GetString() ?? "";
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var arr = JsonSerializer.Deserialize<List<string>>(ref reader, options);
            return arr is { Count: > 0 } ? arr[0] : "";
        }
        reader.Skip();
        return "";
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}

public class ProfileDTO
{
    public long ProfileId { get; set; }

    [JsonConverter(typeof(JobTitleStringJsonConverter))]
    public string SeekedJobTitle { get; set; } = string.Empty;

    public List<string> TechnicalSkills { get; set; } = new List<string>();
    public List<string> Technologies { get; set; } = new();
    public List<string> FieldSkills { get; set; } = new List<string>();
    public List<string> SoftSkills { get; set; } = new List<string>();
    public List<string> JobPositionSkills { get; set; } = new List<string>();
    public string Experience { get; set; } = string.Empty;
    public bool ReceiveNotifications { get; set; } = false;
}
