using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using API.Tests.Helpers;
using FluentAssertions;

namespace API.Tests;

[Collection(nameof(JobAssistantApiCollection))]
public class AdminControllerTests(JobAssistantApiFactory factory)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private async Task<string> CreateAdminTokenAsync(HttpClient client)
    {
        var r = DtoFactory.Register();
        var (_, userId) = await AuthHelper.RegisterAndLoginAsync(client, r.Email, r.Password);
        await AuthHelper.GrantRoleAsync(factory.Services, userId, "Admin");
        AuthHelper.ClearBearer(client);
        var (token, _) = await AuthHelper.LoginAsync(client, r.Email, r.Password);
        return token;
    }

    [Fact]
    public async Task GetJobSources_AdminRole_Returns200()
    {
        using var client = factory.CreateClient();
        var token = await CreateAdminTokenAsync(client);
        AuthHelper.SetBearer(client, token);

        var resp = await client.GetAsync("/api/Admin/job-sources");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var arr = await resp.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        arr.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task GetJobSources_UserRole_Returns403()
    {
        using var client = factory.CreateClient();
        var r = DtoFactory.Register();
        var (token, _) = await AuthHelper.RegisterAndLoginAsync(client, r.Email, r.Password);
        AuthHelper.SetBearer(client, token);

        var resp = await client.GetAsync("/api/Admin/job-sources");
        resp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PatchJobSource_ValidPayload_Returns200()
    {
        using var client = factory.CreateClient();
        var token = await CreateAdminTokenAsync(client);
        AuthHelper.SetBearer(client, token);

        var resp = await client.PatchAsJsonAsync("/api/Admin/job-sources/seeded_source", new { isActive = false }, JsonOptions);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);

        await client.PatchAsJsonAsync("/api/Admin/job-sources/seeded_source", new { isActive = true }, JsonOptions);
    }

    [Fact]
    public async Task PatchJobSource_UnknownSource_Returns404()
    {
        using var client = factory.CreateClient();
        var token = await CreateAdminTokenAsync(client);
        AuthHelper.SetBearer(client, token);

        var resp = await client.PatchAsJsonAsync("/api/Admin/job-sources/unknown_source_xyz", new { isActive = true }, JsonOptions);
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetLogs_ValidContainer_Returns200()
    {
        using var client = factory.CreateClient();
        var token = await CreateAdminTokenAsync(client);
        AuthHelper.SetBearer(client, token);

        var resp = await client.GetAsync("/api/Admin/logs?container=backend");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var text = await resp.Content.ReadAsStringAsync();
        text.Should().Contain("integration-test-log");
    }

    [Fact]
    public async Task GetLogs_InvalidContainer_Returns400()
    {
        using var client = factory.CreateClient();
        var token = await CreateAdminTokenAsync(client);
        AuthHelper.SetBearer(client, token);

        var resp = await client.GetAsync("/api/Admin/logs?container=not_allowed");
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetSettings_AdminRole_Returns200()
    {
        using var client = factory.CreateClient();
        var token = await CreateAdminTokenAsync(client);
        AuthHelper.SetBearer(client, token);

        var resp = await client.GetAsync("/api/Admin/settings");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateSettings_ValidPayload_Returns200()
    {
        using var client = factory.CreateClient();
        var token = await CreateAdminTokenAsync(client);
        AuthHelper.SetBearer(client, token);

        var body = new
        {
            settings = new[]
            {
                new { key = "MinSimilarityThreshold", value = "0.55" },
            },
        };

        var resp = await client.PutAsJsonAsync("/api/Admin/settings", body, JsonOptions);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateSettings_InvalidThreshold_Returns400()
    {
        using var client = factory.CreateClient();
        var token = await CreateAdminTokenAsync(client);
        AuthHelper.SetBearer(client, token);

        var body = new
        {
            settings = new[]
            {
                new { key = "MinSimilarityThreshold", value = "2" },
            },
        };

        var resp = await client.PutAsJsonAsync("/api/Admin/settings", body, JsonOptions);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetAnalytics_AdminRole_Returns200()
    {
        using var client = factory.CreateClient();
        var token = await CreateAdminTokenAsync(client);
        AuthHelper.SetBearer(client, token);

        var resp = await client.GetAsync("/api/Admin/analytics");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var json = await resp.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        json.GetProperty("totalUsers").GetInt32().Should().BeGreaterThanOrEqualTo(0);
    }
}
