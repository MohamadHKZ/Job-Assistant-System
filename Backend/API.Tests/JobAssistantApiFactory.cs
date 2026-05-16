using API.Data;
using API.Entities;
using API.Interfaces;
using JobAssistantSystem.Backend.API.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Testcontainers.PostgreSql;

namespace API.Tests;

public sealed class JobAssistantApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder()
        .WithImage("pgvector/pgvector:pg16")
        .Build();

    public string AdminLogsDirectory { get; } = Path.Combine(Path.GetTempPath(), "jas_admin_logs_" + Guid.NewGuid().ToString("N"));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        var apiContentRoot = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "API"));
        Directory.SetCurrentDirectory(apiContentRoot);
        builder.UseContentRoot(apiContentRoot);

        Directory.CreateDirectory(AdminLogsDirectory);
        File.WriteAllText(Path.Combine(AdminLogsDirectory, "backend.log"), "integration-test-log\n");

        builder.UseEnvironment("Development");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            var baseConn = _postgres.GetConnectionString();
            var conn =
                baseConn.Contains("Maximum Pool Size", StringComparison.OrdinalIgnoreCase) ||
                baseConn.Contains("Max Pool Size", StringComparison.OrdinalIgnoreCase)
                    ? baseConn
                    : $"{baseConn};Pooling=false;Maximum Pool Size=15;Timeout=15;Command Timeout=30";

            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = conn,
                ["Jwt:Key"] = new string('A', 64),
                ["AdminLogs:Directory"] = AdminLogsDirectory,
            }!);
        });

        builder.ConfigureTestServices(services =>
        {
            RemoveExternalHttpServices(services);
            services.AddSingleton<INlpService, TestDoubles.TestNlpService>();
            services.AddSingleton<INlpEmbeddingService, TestDoubles.TestNlpEmbeddingService>();
            services.AddSingleton<IEmbeddingService, TestDoubles.TestEmbeddingService>();
            services.AddSingleton<IMatchingRankingService, TestDoubles.TestMatchingRankingService>();
        });
    }

    private static void RemoveExternalHttpServices(IServiceCollection services)
    {
        foreach (var d in services.ToList())
        {
            if (d.ServiceType == typeof(INlpService) ||
                d.ServiceType == typeof(INlpEmbeddingService) ||
                d.ServiceType == typeof(IEmbeddingService) ||
                d.ServiceType == typeof(IMatchingRankingService))
            {
                services.Remove(d);
            }
        }
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();

        if (!db.Roles.Any())
        {
            db.Roles.AddRange(
                new Role { Name = "User", Description = "Default user" },
                new Role { Name = "Admin", Description = "Administrator" });
            db.SaveChanges();
        }

        if (!db.JobSources.Any())
        {
            db.JobSources.Add(new JobSource { SourceName = "seeded_source", IsActive = true });
            db.SaveChanges();
        }

        return host;
    }

    public async Task InitializeAsync() => await _postgres.StartAsync();

    async Task IAsyncLifetime.DisposeAsync()
    {
        await _postgres.DisposeAsync();
        await base.DisposeAsync().AsTask();
    }
}

[CollectionDefinition(nameof(JobAssistantApiCollection), DisableParallelization = true)]
public class JobAssistantApiCollection : ICollectionFixture<JobAssistantApiFactory>
{
}
