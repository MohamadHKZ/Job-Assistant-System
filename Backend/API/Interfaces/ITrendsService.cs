using API.DTOs;

public interface ITrendsService
{
    Task<IEnumerable<TrendDTO>> GetTrendsAsync();
}
