using System.Security.Claims;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace JobAssistantSystem.API.Errors
{
    /// <summary>
    /// Single entry point for turning every unhandled exception into a
    /// uniform RFC 7807 ProblemDetails response.
    ///
    /// - Known business errors (DomainException) keep their semantic status/title/type.
    /// - Anything else is a 500 with a generic message. The full message + stack trace
    ///   are only surfaced when running in Development.
    /// </summary>
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _env;
        private readonly IProblemDetailsService _problemDetailsService;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IHostEnvironment env,
            IProblemDetailsService problemDetailsService)
        {
            _logger = logger;
            _env = env;
            _problemDetailsService = problemDetailsService;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            ProblemDetails problem;

            var userId = httpContext.User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "anonymous";
            var query = httpContext.Request.QueryString.HasValue ? httpContext.Request.QueryString.Value! : string.Empty;

            if (exception is DomainException domain)
            {
                _logger.LogInformation(
                    "Domain exception handled: {ExceptionType} -> {Status} {Title} on {Method} {Path}{Query} for user {UserId}",
                    domain.GetType().Name, domain.StatusCode, domain.Title,
                    httpContext.Request.Method, httpContext.Request.Path, query, userId);

                problem = new ProblemDetails
                {
                    Status = domain.StatusCode,
                    Title = domain.Title,
                    Type = domain.Type,
                    Detail = domain.Detail
                };
            }
            else
            {
                _logger.LogError(exception,
                    "Unhandled exception {ExceptionType} on {Method} {Path}{Query} for user {UserId}",
                    exception.GetType().Name,
                    httpContext.Request.Method,
                    httpContext.Request.Path,
                    query,
                    userId);

                problem = new ProblemDetails
                {
                    Status = StatusCodes.Status500InternalServerError,
                    Title = "An unexpected error occurred",
                    Type = "https://jobmatch.local/errors/internal-server-error",
                    Detail = _env.IsDevelopment() ? exception.ToString() : null
                };
            }

            httpContext.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;

            return await _problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = problem,
                Exception = exception
            });
        }
    }
}
