using System.Diagnostics;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace API.Middleware;

/// <summary>
/// Adds UserId and TraceId to the logging scope for the remainder of the pipeline (after authentication).
/// </summary>
public sealed class UserIdLoggingScopeMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILoggerFactory _loggerFactory;

    public UserIdLoggingScopeMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
    {
        _next = next;
        _loggerFactory = loggerFactory;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // JwtBearer maps JWT "nameid" to ClaimTypes.NameIdentifier; FindFirst(NameId) would miss it.
        var userId = context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        var logger = _loggerFactory.CreateLogger("RequestScope");
        using (logger.BeginScope("UserId: {UserId}, TraceId: {TraceId}", userId, traceId))
        {
            await _next(context);
        }
    }
}
