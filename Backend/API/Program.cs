using System.Text;
using API.Data;
using API.Interfaces;
using API.Services;
using JobAssistantSystem.API.Interfaces;
using JobAssistantSystem.API.Middleware;
using JobAssistantSystem.API.Services;
using JobAssistantSystem.Backend.API.Interfaces;
using JobAssistantSystem.Backend.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddHttpClient();
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
    dataSourceBuilder.EnableDynamicJson();

    var dataSource = dataSourceBuilder.Build();

    options.UseNpgsql(dataSource);
});
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<INlpService, NlpService>();
builder.Services.AddScoped<INlpEmbeddingService, NlpEmbeddingService>();
builder.Services.AddScoped<IMatchingRankingService, MatchingRankingService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IJobsService, JobsService>();
builder.Services.AddCors();
builder.Services.AddAuthentication((options) =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer((options) =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidateIssuer = false,
        ValidateAudience = false,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("super secret key super secret key"))
    };
});
var app = builder.Build();
app.UseExceptionMiddleware();
app.UseCors((policyConfig) =>
{
    policyConfig.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:5173", "http://frontend:5173", "http://localhost:5004").AllowCredentials();
});
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
