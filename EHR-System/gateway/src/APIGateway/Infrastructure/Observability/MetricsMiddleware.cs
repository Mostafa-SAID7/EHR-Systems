using System.Diagnostics;

namespace EHRPlatform.Gateway.Infrastructure.Observability;

/// <summary>
/// Middleware for recording request metrics.
/// Records request duration, status codes, and active request count.
/// </summary>
public class MetricsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IGatewayMetrics _metrics;
    private readonly ILogger<MetricsMiddleware> _logger;

    public MetricsMiddleware(RequestDelegate next, IGatewayMetrics metrics, ILogger<MetricsMiddleware> logger)
    {
        _next = next;
        _metrics = metrics;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var serviceName = context.Request.Path.Value?.Split('/').ElementAtOrDefault(3) ?? "unknown";
        var endpoint = context.Request.Path.Value ?? "unknown";

        _metrics.RecordActiveRequests(serviceName, 1);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            await _next(context);
        }
        finally
        {
            stopwatch.Stop();

            _metrics.RecordRequestDuration(
                serviceName,
                endpoint,
                stopwatch.ElapsedMilliseconds,
                context.Response.StatusCode);

            _metrics.RecordRequestCount(
                serviceName,
                endpoint,
                context.Response.StatusCode);

            _metrics.RecordActiveRequests(serviceName, -1);
        }
    }
}
