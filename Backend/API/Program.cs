using System.Diagnostics;
using System.Text;
using API.Data;
using API.Http;
using API.Interfaces;
using API.Middleware;
using API.Services;
using JobAssistantSystem.API.Errors;
using JobAssistantSystem.API.Interfaces;
using JobAssistantSystem.API.Services;
using JobAssistantSystem.Backend.API.Interfaces;
using JobAssistantSystem.Backend.API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.HttpLogging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using Microsoft.IdentityModel.Tokens;
using Npgsql;
var builder = WebApplication.CreateBuilder(args);

builder.Logging.Configure(options =>
{
    options.ActivityTrackingOptions = ActivityTrackingOptions.None;
});


builder.Services.AddProblemDetails(options =>
{
    // Inject traceId + instance into every ProblemDetails response so the
    // shape is identical for validation errors, business errors, and 500s.
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Instance ??= ctx.HttpContext.Request.Path;
        ctx.ProblemDetails.Extensions["traceId"] =
            Activity.Current?.Id ?? ctx.HttpContext.TraceIdentifier;
    };
});

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.AddHttpLogging(o =>
{
    o.LoggingFields = HttpLoggingFields.RequestMethod
                    | HttpLoggingFields.RequestPath
                    | HttpLoggingFields.RequestQuery
                    | HttpLoggingFields.ResponseStatusCode
                    | HttpLoggingFields.Duration;
    o.CombineLogs = true;
});

builder.Services.AddControllers().AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddTransient<TraceIdDelegatingHandler>();

builder.Services.AddHttpClient<INlpService, NlpService>()
    .AddHttpMessageHandler<TraceIdDelegatingHandler>();
builder.Services.AddHttpClient<INlpEmbeddingService, NlpEmbeddingService>()
    .AddHttpMessageHandler<TraceIdDelegatingHandler>();
builder.Services.AddHttpClient<IEmbeddingService, EmbeddingService>()
    .AddHttpMessageHandler<TraceIdDelegatingHandler>();
builder.Services.AddHttpClient<IMatchingRankingService, MatchingRankingService>()
    .AddHttpMessageHandler<TraceIdDelegatingHandler>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
    dataSourceBuilder.EnableDynamicJson();

    var dataSource = dataSourceBuilder.Build();

    options.UseNpgsql(dataSource);
});
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IProfileService, ProfileService>();
builder.Services.AddScoped<IJobsService, JobsService>();
builder.Services.AddScoped<ITrendsService, TrendsService>();
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
app.UseExceptionHandler();
app.UseStatusCodePages();
app.UseCors((policyConfig) =>
{
    policyConfig.AllowAnyHeader().AllowAnyMethod().WithOrigins("http://localhost:5173", "http://frontend:5173", "http://localhost:5004").AllowCredentials();
});
app.UseAuthentication();
// Open logging scope first so controller *and* HttpLogging access lines see UserId + TraceId.
app.UseMiddleware<UserIdLoggingScopeMiddleware>();
app.UseHttpLogging();
app.UseAuthorization();
app.MapControllers();

app.Run();
