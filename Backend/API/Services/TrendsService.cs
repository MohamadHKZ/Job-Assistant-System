using API.Data;
using API.DTOs;
using API.Entities;
using Microsoft.EntityFrameworkCore;

class TrendsService(AppDbContext _dbContext) : ITrendsService
{
    public async Task<IEnumerable<TrendDTO>> GetTrendsAsync()
    {
        var trends = await _dbContext.Trends
            .AsNoTracking()
            .Where(t => t.JobsCount > 0)
            .OrderByDescending(t => t.JobsCount)
            .ToListAsync();

        var jobRatios = RoundToHundredthsSummingToOne(
            trends.Select(t => t.JobsCount).ToList());

        return trends
            .Select((t, i) => MapToDto(t, jobRatios[i]))
            .ToList();
    }

    private static TrendDTO MapToDto(Trend trend, double jobRatio)
    {
        return new TrendDTO
        {
            JobTitle = trend.JobTitle,
            JobsCount = trend.JobsCount,
            JobRatio = jobRatio,
            TopTechnicalSkills = new TrendTopSkillsDTO
            {
                Week = MapPeriod(trend.TopTechnicalSkills?.Week),
                Month = MapPeriod(trend.TopTechnicalSkills?.Month),
                ThreeMonths = MapPeriod(trend.TopTechnicalSkills?.ThreeMonths),
            }
        };
    }

    private static TrendPeriodSkillsDTO MapPeriod(TrendPeriodSkills? period)
    {
        if (period is null)
        {
            return new TrendPeriodSkillsDTO();
        }

        var skillRatios = RoundToHundredthsSummingToOne(
            period.TopSkills.Select(s => s.Count).ToList());

        return new TrendPeriodSkillsDTO
        {
            TotalSkills = period.TotalSkills,
            TopSkills = period.TopSkills
                .Select((s, i) => new TrendSkillCountDTO
                {
                    Skill = s.Skill,
                    Count = s.Count,
                    Ratio = skillRatios[i],
                })
                .ToList(),
        };
    }

    /// <summary>
    /// Distributes 100 hundredths across the input counts using the
    /// largest-remainder (Hamilton) method, then divides by 100 to return
    /// ratios with exactly two decimals that sum to exactly 1.00.
    /// Returns an array of zeros if the input is empty or sums to zero.
    /// </summary>
    private static double[] RoundToHundredthsSummingToOne(IReadOnlyList<int> counts)
    {
        var result = new double[counts.Count];
        var total = counts.Sum();
        if (total <= 0)
        {
            return result;
        }

        var floorHundredths = new int[counts.Count];
        var remainders = new (double Fraction, int Index)[counts.Count];

        for (int i = 0; i < counts.Count; i++)
        {
            var exact = 100.0 * counts[i] / total;
            var floor = (int)Math.Floor(exact);
            floorHundredths[i] = floor;
            remainders[i] = (exact - floor, i);
        }

        var deficit = 100 - floorHundredths.Sum();

        var ordered = remainders
            .OrderByDescending(r => r.Fraction)
            .ThenByDescending(r => counts[r.Index])
            .ThenBy(r => r.Index)
            .ToArray();

        for (int k = 0; k < deficit; k++)
        {
            floorHundredths[ordered[k].Index] += 1;
        }

        for (int i = 0; i < counts.Count; i++)
        {
            result[i] = floorHundredths[i] / 100.0;
        }

        return result;
    }
}
