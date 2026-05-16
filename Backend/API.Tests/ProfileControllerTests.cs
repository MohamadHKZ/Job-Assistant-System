using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using API.Tests.Helpers;
using FluentAssertions;

namespace API.Tests;

[Collection(nameof(JobAssistantApiCollection))]
public class ProfileControllerTests(JobAssistantApiFactory factory)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [Fact]
    public async Task GetProfile_ValidId_Returns200WithData()
    {
        using var client = factory.CreateClient();
        var r = DtoFactory.Register();
        var (token, userId) = await AuthHelper.RegisterAndLoginAsync(client, r.Email, r.Password);
        AuthHelper.SetBearer(client, token);

        var cfg = DtoFactory.ProfileConfig();
        var saveResp = await client.PostAsJsonAsync($"/api/Profile/{userId}/save", DtoFactory.ToJsonBody(cfg), JsonOptions);
        saveResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var profileId = (await saveResp.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("profileId").GetInt32();

        var getResp = await client.GetAsync($"/api/Profile/{profileId}");
        getResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await getResp.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("profileId").GetInt32().Should().Be(profileId);
    }

    [Fact]
    public async Task GetProfile_NotFound_Returns404()
    {
        using var client = factory.CreateClient();
        var r = DtoFactory.Register();
        var (token, _) = await AuthHelper.RegisterAndLoginAsync(client, r.Email, r.Password);
        AuthHelper.SetBearer(client, token);

        var resp = await client.GetAsync("/api/Profile/999999");
        resp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetProfile_Unauthenticated_Returns401()
    {
        using var client = factory.CreateClient();
        AuthHelper.ClearBearer(client);
        var resp = await client.GetAsync("/api/Profile/1");
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProfileIdByUserId_Returns200()
    {
        using var client = factory.CreateClient();
        var r = DtoFactory.Register();
        var (token, userId) = await AuthHelper.RegisterAndLoginAsync(client, r.Email, r.Password);
        AuthHelper.SetBearer(client, token);

        var cfg = DtoFactory.ProfileConfig();
        var saveResp = await client.PostAsJsonAsync($"/api/Profile/{userId}/save", DtoFactory.ToJsonBody(cfg), JsonOptions);
        var profileId = (await saveResp.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("profileId").GetInt32();

        var resp = await client.GetAsync($"/api/Profile/{userId}/profile_id");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var id = int.Parse(await resp.Content.ReadAsStringAsync());
        id.Should().Be(profileId);
    }

    [Fact]
    public async Task SaveProfile_NewProfile_Returns200()
    {
        using var client = factory.CreateClient();
        var r = DtoFactory.Register();
        var (token, userId) = await AuthHelper.RegisterAndLoginAsync(client, r.Email, r.Password);
        AuthHelper.SetBearer(client, token);

        var resp = await client.PostAsJsonAsync($"/api/Profile/{userId}/save", DtoFactory.ToJsonBody(DtoFactory.ProfileConfig()), JsonOptions);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SaveProfile_Unauthenticated_Returns401()
    {
        using var client = factory.CreateClient();
        AuthHelper.ClearBearer(client);
        var resp = await client.PostAsJsonAsync("/api/Profile/1/save", DtoFactory.ToJsonBody(DtoFactory.ProfileConfig()), JsonOptions);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateProfile_ExistingProfile_Returns200()
    {
        using var client = factory.CreateClient();
        var r = DtoFactory.Register();
        var (token, userId) = await AuthHelper.RegisterAndLoginAsync(client, r.Email, r.Password);
        AuthHelper.SetBearer(client, token);

        var saveResp = await client.PostAsJsonAsync($"/api/Profile/{userId}/save", DtoFactory.ToJsonBody(DtoFactory.ProfileConfig()), JsonOptions);
        var profileId = (await saveResp.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("profileId").GetInt32();

        var updateCfg = DtoFactory.ProfileConfig("Lead Engineer");
        var putResp = await client.PutAsJsonAsync($"/api/Profile/{profileId}/update", DtoFactory.ToJsonBody(updateCfg), JsonOptions);
        putResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateProfile_AsDifferentUser_Returns200()
    {
        using var client = factory.CreateClient();
        var r1 = DtoFactory.Register();
        var (token1, userId1) = await AuthHelper.RegisterAndLoginAsync(client, r1.Email, r1.Password);
        AuthHelper.SetBearer(client, token1);
        var saveResp = await client.PostAsJsonAsync($"/api/Profile/{userId1}/save", DtoFactory.ToJsonBody(DtoFactory.ProfileConfig()), JsonOptions);
        var profileId = (await saveResp.Content.ReadFromJsonAsync<JsonElement>(JsonOptions)).GetProperty("profileId").GetInt32();

        var r2 = DtoFactory.Register();
        var (token2, _) = await AuthHelper.RegisterAndLoginAsync(client, r2.Email, r2.Password);
        AuthHelper.SetBearer(client, token2);

        var putResp = await client.PutAsJsonAsync($"/api/Profile/{profileId}/update", DtoFactory.ToJsonBody(DtoFactory.ProfileConfig("Other Title")), JsonOptions);
        putResp.StatusCode.Should().Be(HttpStatusCode.OK, "API currently does not restrict profile updates to the owning user");
    }

    [Fact]
    public async Task ExtractInfo_ValidPdf_Returns200WithStructuredData()
    {
        using var client = factory.CreateClient();
        var r = DtoFactory.Register();
        var (token, userId) = await AuthHelper.RegisterAndLoginAsync(client, r.Email, r.Password);
        AuthHelper.SetBearer(client, token);

        using var content = new ByteArrayContent(PdfTestData.CreateCvPdfBytes());
        content.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");

        var resp = await client.PostAsync($"/api/Profile/extract-info/{userId}", content);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("seekedJobTitle").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task ExtractInfo_InvalidContentType_Returns415()
    {
        using var client = factory.CreateClient();
        var r = DtoFactory.Register();
        var (token, userId) = await AuthHelper.RegisterAndLoginAsync(client, r.Email, r.Password);
        AuthHelper.SetBearer(client, token);

        var resp = await client.PostAsJsonAsync($"/api/Profile/extract-info/{userId}", new { a = 1 }, JsonOptions);
        resp.StatusCode.Should().Be(HttpStatusCode.UnsupportedMediaType);
    }
}
