using System.Diagnostics;

namespace API.Http;

/// <summary>
/// Adds X-Trace-Id to outbound HTTP calls so Python services can correlate logs with the ASP.NET request.
/// </summary>
public sealed class TraceIdDelegatingHandler : DelegatingHandler
{
    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var traceId = Activity.Current?.Id;
        if (string.IsNullOrEmpty(traceId))
            traceId = Activity.Current?.TraceId.ToString();

        if (string.IsNullOrEmpty(traceId))
            traceId = Guid.NewGuid().ToString("N");

        if (!request.Headers.Contains("X-Trace-Id"))
            request.Headers.TryAddWithoutValidation("X-Trace-Id", traceId);

        return base.SendAsync(request, cancellationToken);
    }
}
