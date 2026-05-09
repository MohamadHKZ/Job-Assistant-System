using API.Data;
using API.DTOs;
using API.Entities;
using JobAssistantSystem.API.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Globalization;

namespace API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
public sealed class AdminController : BaseController
{
    private readonly AppDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AdminController> _logger;

    private static readonly HashSet<string> AllowedLogContainers = new(StringComparer.OrdinalIgnoreCase)
    {
        "backend",
        "nlp_service",
        "matching_service",
        "embedding_service",
        "nlp_embedding_service",
        "job_collector_orchestrator",
        "log_collector",
    };

    private static readonly HashSet<string> AllowedSettingKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "ScrapingInterval",
        "MinSimilarityThreshold",
    };

    private static readonly HashSet<string> AllowedScrapeIntervals = new(StringComparer.OrdinalIgnoreCase)
    {
        "LAST_DAY",
        "LAST_WEEK",
        "LAST_MONTH",
    };

    public AdminController(AppDbContext db, IConfiguration configuration, ILogger<AdminController> logger)
    {
        _db = db;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("job-sources")]
    public async Task<ActionResult<IEnumerable<JobSourceAdminDto>>> GetJobSources(CancellationToken ct)
    {
        var list = await _db.JobSources
            .AsNoTracking()
            .OrderBy(j => j.SourceName)
            .Select(j => new JobSourceAdminDto { SourceName = j.SourceName, IsActive = j.IsActive })
            .ToListAsync(ct);
        return Ok(list);
    }

    [HttpPatch("job-sources/{sourceName}")]
    public async Task<ActionResult<JobSourceAdminDto>> PatchJobSource(string sourceName, [FromBody] PatchJobSourceDto body, CancellationToken ct)
    {
        var name = Uri.UnescapeDataString(sourceName ?? "");
        var entity = await _db.JobSources.FirstOrDefaultAsync(j => j.SourceName == name, ct);
        if (entity is null)
            throw new NotFoundException("job source", name);

        entity.IsActive = body.IsActive;
        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("Job source {SourceName} IsActive set to {IsActive}", name, body.IsActive);
        return Ok(new JobSourceAdminDto { SourceName = entity.SourceName, IsActive = entity.IsActive });
    }

    [HttpGet("logs")]
    public IActionResult GetLogs([FromQuery] string container)
    {
        if (string.IsNullOrWhiteSpace(container) || !AllowedLogContainers.Contains(container))
        {
            var list = string.Join(", ", AllowedLogContainers.OrderBy(x => x));
            var detail = string.IsNullOrWhiteSpace(container)
                ? $"Query parameter 'container' is required. Allowed values: {list}."
                : $"Unknown container '{container.Trim()}'. Allowed values: {list}.";
            return this.ProblemFor(
                typeSlug: "invalid-admin-log-container",
                title: "Invalid log container",
                statusCode: StatusCodes.Status400BadRequest,
                detail: detail);
        }

        var dir = _configuration["AdminLogs:Directory"] ?? "/logs";
        var safeName = Path.GetFileName(container.Trim());
        var path = Path.Combine(dir, $"{safeName}.log");

        try
        {
            if (!System.IO.File.Exists(path))
            {
                return Content(
                    $"No log file yet for '{safeName}'. Ensure log_collector is running and has written at least once.",
                    "text/plain");
            }

            var text = System.IO.File.ReadAllText(path);
            return Content(text, "text/plain");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read log file {Path}", path);
            return this.ProblemFor(
                typeSlug: "admin-log-read",
                title: "Log read failed",
                statusCode: StatusCodes.Status500InternalServerError,
                detail: $"Could not read log file '{safeName}'.");
        }
    }

    [HttpGet("settings")]
    public async Task<ActionResult<IEnumerable<SystemSettingDto>>> GetSettings(CancellationToken ct)
    {
        var rows = await _db.SystemSettings
            .AsNoTracking()
            .OrderBy(s => s.Key)
            .Select(s => new SystemSettingDto { Key = s.Key, Value = s.Value, UpdatedAt = s.UpdatedAt })
            .ToListAsync(ct);
        return Ok(rows);
    }

    [HttpPut("settings")]
    public async Task<ActionResult<IEnumerable<SystemSettingDto>>> PutSettings([FromBody] UpdateSystemSettingsDto body, CancellationToken ct)
    {
        if (body.Settings is null || body.Settings.Count == 0)
            return this.ProblemFor(
                typeSlug: "invalid-admin-settings",
                title: "Invalid settings",
                statusCode: StatusCodes.Status400BadRequest,
                detail: "Provide at least one setting.");

        foreach (var item in body.Settings)
        {
            var key = item.Key?.Trim() ?? "";
            if (!AllowedSettingKeys.Contains(key))
                return this.ProblemFor(
                    typeSlug: "invalid-admin-settings",
                    title: "Invalid settings",
                    statusCode: StatusCodes.Status400BadRequest,
                    detail: $"Key '{item.Key}' is not allowed.");

            if (string.Equals(key, "ScrapingInterval", StringComparison.OrdinalIgnoreCase))
            {
                var v = item.Value?.Trim() ?? "";
                if (!AllowedScrapeIntervals.Contains(v))
                    return this.ProblemFor(
                        typeSlug: "invalid-admin-settings",
                        title: "Invalid settings",
                        statusCode: StatusCodes.Status400BadRequest,
                        detail: "ScrapingInterval must be LAST_DAY, LAST_WEEK, or LAST_MONTH.");
            }
            else if (string.Equals(key, "MinSimilarityThreshold", StringComparison.OrdinalIgnoreCase))
            {
                if (!double.TryParse(item.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var d) || d < 0 || d > 1)
                    return this.ProblemFor(
                        typeSlug: "invalid-admin-settings",
                        title: "Invalid settings",
                        statusCode: StatusCodes.Status400BadRequest,
                        detail: "MinSimilarityThreshold must be a number between 0 and 1.");
            }
        }

        var utc = DateTime.UtcNow;
        foreach (var item in body.Settings)
        {
            var key = item.Key?.Trim() ?? "";
            var entity = await _db.SystemSettings.FirstOrDefaultAsync(s => s.Key == key, ct);
            if (entity is null)
            {
                entity = new SystemSetting { Key = key, Value = item.Value?.Trim() ?? "", UpdatedAt = utc };
                _db.SystemSettings.Add(entity);
            }
            else
            {
                entity.Value = item.Value?.Trim() ?? "";
                entity.UpdatedAt = utc;
            }
        }

        await _db.SaveChangesAsync(ct);
        _logger.LogInformation("System settings updated: {Keys}", string.Join(", ", body.Settings.Select(s => s.Key)));

        var rows = await _db.SystemSettings
            .AsNoTracking()
            .OrderBy(s => s.Key)
            .Select(s => new SystemSettingDto { Key = s.Key, Value = s.Value, UpdatedAt = s.UpdatedAt })
            .ToListAsync(ct);
        return Ok(rows);
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<AdminAnalyticsDto>> GetAnalytics(CancellationToken ct)
    {
        var totalUsers = await _db.Users.AsNoTracking().CountAsync(ct);
        var totalJobs = await _db.JobPosts.AsNoTracking().CountAsync(ct);

        var trends = await _db.Trends.AsNoTracking().ToListAsync(ct);
        var agg = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        foreach (var t in trends)
        {
            var monthSkills = t.TopTechnicalSkills?.Month?.TopSkills;
            if (monthSkills is null)
                continue;
            foreach (var s in monthSkills)
            {
                if (string.IsNullOrWhiteSpace(s.Skill))
                    continue;
                agg.TryGetValue(s.Skill.Trim(), out var c);
                agg[s.Skill.Trim()] = c + s.Count;
            }
        }

        var topSkills = agg
            .OrderByDescending(kv => kv.Value)
            .Take(20)
            .Select(kv => new TrendingSkillAdminDto { Skill = kv.Key, Count = kv.Value })
            .ToList();

        return Ok(new AdminAnalyticsDto
        {
            TotalUsers = totalUsers,
            TotalMatchedJobs = totalJobs,
            TrendingSkills = topSkills,
        });
    }
}
