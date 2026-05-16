using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using API.Data;
using API.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace API.Tests;

[Collection(nameof(JobAssistantApiCollection))]
public class JobsControllerTests(JobAssistantApiFactory factory)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [Fact]
    public async Task GetJobsByProfile_ValidProfileId_Returns200WithPaginatedList()
    {
        using var client = factory.CreateClient();
        var r = DtoFactory.Register();
        var (token, userId) = await AuthHelper.RegisterAndLoginAsync(client, r.Email, r.Password);
        AuthHelper.SetBearer(client, token);

        var saveResp = await client.PostAsJsonAsync($"/api/Profile/{userId}/save", DtoFactory.ToJsonBody(DtoFactory.ProfileConfig()), JsonOptions);
        saveResp.EnsureSuccessStatusCode();
        var profileId = (await saveResp.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("profileId").GetInt32();

        var sourceName = $"jobs_{Guid.NewGuid():N}";
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await JobsDbSeedHelper.SeedEmbeddedJobPostsAsync(db, sourceName, 7);
        }

        var jobsResp = await client.GetAsync($"/api/Jobs/{profileId}");
        jobsResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var page = await jobsResp.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        page.GetProperty("jobs").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetJobsByProfile_WithCursor_Returns200NextPage()
    {
        using var client = factory.CreateClient();
        var r = DtoFactory.Register();
        var (token, userId) = await AuthHelper.RegisterAndLoginAsync(client, r.Email, r.Password);
        AuthHelper.SetBearer(client, token);

        var saveResp = await client.PostAsJsonAsync($"/api/Profile/{userId}/save", DtoFactory.ToJsonBody(DtoFactory.ProfileConfig()), JsonOptions);
        saveResp.EnsureSuccessStatusCode();
        var profileId = (await saveResp.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("profileId").GetInt32();

        var sourceName = $"jobs_{Guid.NewGuid():N}";
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await JobsDbSeedHelper.SeedEmbeddedJobPostsAsync(db, sourceName, 7);
        }

        var first = await client.GetAsync($"/api/Jobs/{profileId}");
        first.EnsureSuccessStatusCode();
        var firstJson = await first.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        firstJson.GetProperty("hasNextPage").GetBoolean().Should().BeTrue();

        var nextScore = firstJson.GetProperty("nextCursorScore").GetDouble().ToString(CultureInfo.InvariantCulture);
        var nextId = firstJson.GetProperty("nextCursorId").GetGuid();

        var second = await client.GetAsync($"/api/Jobs/{profileId}?cursorScore={nextScore}&cursorId={nextId}");
        second.StatusCode.Should().Be(HttpStatusCode.OK);
        var secondJson = await second.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        secondJson.GetProperty("jobs").GetArrayLength().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetJobsByProfile_Unauthenticated_Returns401()
    {
        using var client = factory.CreateClient();
        AuthHelper.ClearBearer(client);
        var resp = await client.GetAsync("/api/Jobs/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetJobById_Returns200()
    {
        using var client = factory.CreateClient();
        var r = DtoFactory.Register();
        var (token, userId) = await AuthHelper.RegisterAndLoginAsync(client, r.Email, r.Password);
        AuthHelper.SetBearer(client, token);

        var sourceName = $"jobs_{Guid.NewGuid():N}";
        Guid jobId;
        using (var scope = factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            await JobsDbSeedHelper.SeedEmbeddedJobPostsAsync(db, sourceName, 1);
            jobId = await db.EmbeddedJobPosts.Where(e => e.SourceName == sourceName).Select(e => e.Id).FirstAsync();
        }

        var resp = await client.GetAsync($"/api/Jobs?jobId={jobId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.ValueKind.Should().Be(JsonValueKind.Object);
    }
}
