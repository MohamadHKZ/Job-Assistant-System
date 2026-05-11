using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

public class MatchResultIdJsonConverter : JsonConverter<string>
{
    public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        return reader.TokenType switch
        {
            JsonTokenType.String => reader.GetString() ?? "",
            JsonTokenType.Number when reader.TryGetInt64(out var l) => l.ToString(CultureInfo.InvariantCulture),
            _ => "",
        };
    }

    public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value);
    }
}

public class MatchResultDTO
{
    [JsonPropertyName("id")]
    [JsonConverter(typeof(MatchResultIdJsonConverter))]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("match_percentage")]
    public double Score { get; set; }
}
