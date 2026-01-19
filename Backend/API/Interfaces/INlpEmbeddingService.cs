namespace API.Interfaces
{
    public interface INlpEmbeddingService
    {
        Task<MatchingWithEmbeddingDTO[]> StructureAndEmbed(string prompt);
    }
}
