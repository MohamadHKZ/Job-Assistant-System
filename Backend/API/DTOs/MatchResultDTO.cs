using System.Text.Json.Serialization;

public class MatchResultDTO
{
    [JsonPropertyName("id")]
    public long Id { get; set; }
    [JsonPropertyName("match_percentage")]
    public double Score { get; set; }
}