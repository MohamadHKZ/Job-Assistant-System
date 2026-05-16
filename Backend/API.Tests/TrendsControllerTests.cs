using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using API.Tests.Helpers;
using FluentAssertions;

namespace API.Tests;

[Collection(nameof(JobAssistantApiCollection))]
public class TrendsControllerTests(JobAssistantApiFactory factory)
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    [Fact]
    public async Task GetTrends_Anonymous_Returns200()
    {
        using var client = factory.CreateClient();
        AuthHelper.ClearBearer(client);

        var resp = await client.GetAsync("/api/Trends");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await resp.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        data.ValueKind.Should().Be(JsonValueKind.Array);
    }

    [Fact]
    public async Task GetTrends_Authenticated_Returns200()
    {
        using var client = factory.CreateClient();
        var r = DtoFactory.Register();
        var (token, _) = await AuthHelper.RegisterAndLoginAsync(client, r.Email, r.Password);
        AuthHelper.SetBearer(client, token);

        var resp = await client.GetAsync("/api/Trends");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await resp.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        data.ValueKind.Should().Be(JsonValueKind.Array);
    }
}
