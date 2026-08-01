using System.Diagnostics;
using EHRPlatform.BuildingBlocks.Observability.Telemetry;
using Serilog.Context;

namespace EHRPlatform.Services.ApiGateway.Infrastructure.Middleware;

/// <summary>
/// Gateway-level request tracking middleware.
///
/// Responsibilities:
///   - Correlation ID propagation (read / generate / echo X-Correlation-ID header)
///   - Latency measurement and structured logging
///   - HIPAA-safe path scrubbing (patient IDs redacted before logging)
///   - OpenTelemetry span annotation with correlation ID and scrubbed path
///
/// Mount BEFORE UseAuthentication so every request — including 401s — is tracked.
/// </summary>
public sealed class RequestTrackingMiddleware
{
    private const string CorrelationIdHeader = "X-Correlation-ID";

    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTrackingMiddleware> _logger;

    // Route segments after which the next path token is a resource ID (redact it)
    private static readonly HashSet<string> _piiSegments = new(StringComparer.OrdinalIgnoreCase)
    {
        "patients", "clinical", "prescriptions", "appointments",
        "billing", "records", "audit", "analytics", "notifications"
    };

    // Paths skipped from tracking (health / liveness probes)
    private static readonly string[] _skipPrefixes = ["/health", "/metrics", "/favicon"];

    public RequestTrackingMiddleware(
        RequestDelegate next,
        ILogger<RequestTrackingMiddleware> logger)
    {
        _next   = next   ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "/";

        // Skip probe endpoints
        if (_skipPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        // Resolve or generate correlation ID
        var correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeader, out var incoming)
            && !string.IsNullOrWhiteSpace(incoming)
            ? incoming.ToString()
            : Guid.NewGuid().ToString("D");

        context.Items["CorrelationId"] = correlationId;
        context.TraceIdentifier        = correlationId;

        // Echo back in response
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        var scrubbedPath = ScrubPath(path);
        var sw           = Stopwatch.StartNew();

        // Annotate the active OTel span (if any)
        var activity = System.Diagnostics.Activity.Current;
        activity?.SetTag(EHRTelemetry.TagCorrelationId, correlationId);
        activity?.SetTag("http.route.scrubbed", scrubbedPath);

        using (LogContext.PushProperty("CorrelationId", correlationId))
        using (LogContext.PushProperty("GatewayPath",   scrubbedPath))
        {
            _logger.LogInformation(
                "GW → {Method} {ScrubbedPath}",
                context.Request.Method, scrubbedPath);

            try
            {
                await _next(context);
            }
            finally
            {
                sw.Stop();
                var status = context.Response.StatusCode;
                var level  = status >= 500 ? LogLevel.Error
                           : status >= 400 ? LogLevel.Warning
                           : LogLevel.Information;

                _logger.Log(level,
                    "GW ← {Method} {ScrubbedPath} {StatusCode} {ElapsedMs}ms [CorrelationId={CorrelationId}]",
                    context.Request.Method, scrubbedPath, status,
                    sw.ElapsedMilliseconds, correlationId);
            }
        }
    }

    /// <summary>
    /// Replace resource IDs that follow PHI-bearing route segments with [ID].
    /// /api/v1/patients/abc-123/records → /api/v1/patients/[ID]/records
    /// </summary>
    private static string ScrubPath(string path)
    {
        var segments  = path.Split('/', StringSplitOptions.None);
        var result    = new List<string>(segments.Length);
        var scrubNext = false;

        foreach (var segment in segments)
        {
            if (scrubNext && segment.Length > 0)
            {
                result.Add("[ID]");
                scrubNext = false;
            }
            else
            {
                result.Add(segment);
                scrubNext = _piiSegments.Contains(segment);
            }
        }

        return string.Join("/", result);
    }
}

public static class RequestTrackingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestTracking(this IApplicationBuilder builder)
        => builder.UseMiddleware<RequestTrackingMiddleware>();
}
