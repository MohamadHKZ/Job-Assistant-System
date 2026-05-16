using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using API.Data;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace API.Tests.Helpers;

public static class AuthHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public static async Task<(string Token, int UserId)> RegisterAndLoginAsync(HttpClient client, string email, string password, string firstName = "Test", string lastName = "User")
    {
        var register = new
        {
            firstName,
            lastName,
            email,
            password,
        };
        var regResp = await client.PostAsJsonAsync("/api/Accounts/register", register, JsonOptions);
        regResp.EnsureSuccessStatusCode();
        var regJson = await regResp.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var userId = regJson.GetProperty("jobSeekerId").GetInt32();

        var login = new { email, password };
        var loginResp = await client.PostAsJsonAsync("/api/Accounts/login", login, JsonOptions);
        loginResp.EnsureSuccessStatusCode();
        var loginJson = await loginResp.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var token = loginJson.GetProperty("token").GetString() ?? "";

        return (token, userId);
    }

    public static async Task<(string Token, int UserId)> LoginAsync(HttpClient client, string email, string password)
    {
        var login = new { email, password };
        var loginResp = await client.PostAsJsonAsync("/api/Accounts/login", login, JsonOptions);
        loginResp.EnsureSuccessStatusCode();
        var loginJson = await loginResp.Content.ReadFromJsonAsync<JsonElement>(JsonOptions);
        var token = loginJson.GetProperty("token").GetString() ?? "";
        var userId = loginJson.GetProperty("jobSeekerId").GetInt32();
        return (token, userId);
    }

    public static void SetBearer(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public static void ClearBearer(HttpClient client) =>
        client.DefaultRequestHeaders.Authorization = null;

    public static async Task GrantRoleAsync(IServiceProvider services, int userId, string roleName)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var role = await db.Roles.AsNoTracking().FirstAsync(r => r.Name == roleName);
        if (await db.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == role.Id))
            return;

        db.UserRoles.Add(new UserRole
        {
            UserId = userId,
            RoleId = role.Id,
            AssignedAt = DateTime.UtcNow,
        });
        await db.SaveChangesAsync();
    }
}
