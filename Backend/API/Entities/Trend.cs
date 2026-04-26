using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace API.Entities;

[Table("Trends")]
public class Trend
{
    [Key]
    public int Id { get; set; }

    public string JobTitle { get; set; } = string.Empty;

    public string? JobTitlePattern { get; set; }

    public int JobsCount { get; set; }

    [Column(TypeName = "jsonb")]
    public TrendTopTechnicalSkills? TopTechnicalSkills { get; set; }
}

public class TrendTopTechnicalSkills
{
    [JsonPropertyName("week")]
    public TrendPeriodSkills? Week { get; set; }

    [JsonPropertyName("month")]
    public TrendPeriodSkills? Month { get; set; }

    [JsonPropertyName("3_months")]
    public TrendPeriodSkills? ThreeMonths { get; set; }
}

public class TrendPeriodSkills
{
    [JsonPropertyName("total_skills")]
    public int TotalSkills { get; set; }

    [JsonPropertyName("top_skills")]
    public List<TrendSkillCount> TopSkills { get; set; } = new();
}

public class TrendSkillCount
{
    [JsonPropertyName("skill")]
    public string Skill { get; set; } = string.Empty;

    [JsonPropertyName("count")]
    public int Count { get; set; }
}
