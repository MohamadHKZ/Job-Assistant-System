using API.DTOs;
using API.Interfaces;

namespace API.Tests.TestDoubles;

public sealed class TestNlpEmbeddingService : INlpEmbeddingService
{
    public Task<MatchingWithEmbeddingDTO[]> StructureAndEmbed(string prompt)
        => Task.FromResult(Array.Empty<MatchingWithEmbeddingDTO>());
}
