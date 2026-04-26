namespace API.DTOs;

public class TrendDTO
{
    public string JobTitle { get; set; } = string.Empty;
    public int JobsCount { get; set; }
    public double JobRatio { get; set; }
    public TrendTopSkillsDTO TopTechnicalSkills { get; set; } = new();
}

public class TrendTopSkillsDTO
{
    public TrendPeriodSkillsDTO Week { get; set; } = new();
    public TrendPeriodSkillsDTO Month { get; set; } = new();
    public TrendPeriodSkillsDTO ThreeMonths { get; set; } = new();
}

public class TrendPeriodSkillsDTO
{
    public int TotalSkills { get; set; }
    public List<TrendSkillCountDTO> TopSkills { get; set; } = new();
}

public class TrendSkillCountDTO
{
    public string Skill { get; set; } = string.Empty;
    public int Count { get; set; }
    public double Ratio { get; set; }
}
