using System.Text.Json.Serialization;
using API.Entities;

public class MatchingWithEmbeddingDTO
{
    [JsonPropertyName("entity")]
    public MatchingObject MatchingObject { get; set; } = new();
    [JsonPropertyName("embeddings")]
    public EmbeddingEntity Embedding { get; set; } = new();
}