using System.Text.Json.Serialization;
using API.Entities;

namespace API.DTOs;

public class MatchingObjectDTO
{
    [JsonPropertyName("id")]
    public long Id { get; set; }

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("experience")]
    public string Experience { get; set; } = string.Empty;

    [JsonPropertyName("techonologies")]
    public List<string> Techonologies { get; set; } = new();

    [JsonPropertyName("embeddings")]
    public EmbeddingCategories Embeddings { get; set; } = new();
}