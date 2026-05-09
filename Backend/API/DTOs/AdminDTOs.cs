namespace API.DTOs;

public class JobSourceAdminDto
{
    public string SourceName { get; set; } = "";
    public bool IsActive { get; set; }
}

public class PatchJobSourceDto
{
    public bool IsActive { get; set; }
}

public class SystemSettingDto
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
    public DateTime UpdatedAt { get; set; }
}

public class SystemSettingUpdateItemDto
{
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
}

public class UpdateSystemSettingsDto
{
    public List<SystemSettingUpdateItemDto> Settings { get; set; } = new();
}

public class AdminAnalyticsDto
{
    public int TotalUsers { get; set; }

    /// <summary>Total job posts stored (proxy for matched/catalogued jobs).</summary>
    public int TotalMatchedJobs { get; set; }

    public List<TrendingSkillAdminDto> TrendingSkills { get; set; } = new();
}

public class TrendingSkillAdminDto
{
    public string Skill { get; set; } = "";
    public int Count { get; set; }
}
