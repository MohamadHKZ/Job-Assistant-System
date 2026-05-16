using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using API.Tests.Helpers;
using FluentAssertions;

namespace API.Tests;

[Collection(nameof(JobAssistantApiCollection))]
public class AccountsControllerTests(JobAssistantApiFactory factory)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [Fact]
    public async Task Register_ValidPayload_Returns200()
    {
        using var client = factory.CreateClient();
        var r = DtoFactory.Register();
        var resp = await client.PostAsJsonAsync("/api/Accounts/register", new
        {
            r.FirstName,
            r.LastName,
            r.Email,
            r.Password,
        }, JsonOptions);

        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("jobSeekerId").GetInt32().Should().BeGreaterThan(0);
        body.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_DuplicateEmail_Returns409()
    {
        using var client = factory.CreateClient();
        var r = DtoFactory.Register();
        var payload = new { r.FirstName, r.LastName, r.Email, r.Password };
        (await client.PostAsJsonAsync("/api/Accounts/register", payload, JsonOptions)).EnsureSuccessStatusCode();

        var resp = await client.PostAsJsonAsync("/api/Accounts/register", payload, JsonOptions);
        resp.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_MissingRequiredFields_Returns400()
    {
        using var client = factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/Accounts/register", new { email = "only@email.com" }, JsonOptions);
        resp.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_ValidCredentials_ReturnsTokenAndUserId()
    {
        using var client = factory.CreateClient();
        var r = DtoFactory.Register();
        await client.PostAsJsonAsync("/api/Accounts/register", new { r.FirstName, r.LastName, r.Email, r.Password }, JsonOptions);

        var resp = await client.PostAsJsonAsync("/api/Accounts/login", new { r.Email, r.Password }, JsonOptions);
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        body.GetProperty("token").GetString().Should().NotBeNullOrEmpty();
        body.GetProperty("jobSeekerId").GetInt32().Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Login_WrongPassword_Returns401()
    {
        using var client = factory.CreateClient();
        var r = DtoFactory.Register();
        await client.PostAsJsonAsync("/api/Accounts/register", new { r.FirstName, r.LastName, r.Email, r.Password }, JsonOptions);

        var resp = await client.PostAsJsonAsync("/api/Accounts/login", new { r.Email, password = "WrongPassword!" }, JsonOptions);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_NonExistentUser_Returns401()
    {
        using var client = factory.CreateClient();
        var resp = await client.PostAsJsonAsync("/api/Accounts/login", new { email = "nobody@example.com", password = "AnyPassword1!" }, JsonOptions);
        resp.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
