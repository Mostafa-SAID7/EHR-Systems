#nullable enable

using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace EHRPlatform.Common.Shared.Middleware;

/// <summary>
/// Structured request/response logging middleware for EHR microservices.
///
/// Logs:
///   - Incoming: method, path (PII-scrubbed), content-type, user-id (opaque)
///   - Outgoing: status code, elapsed ms
///
/// HIPAA:
///   - Patient identifiers in URL path segments are replaced with [REDACTED]
///   - Request/response bodies are NEVER logged (may contain PHI)
///   - User email and name are NOT logged; only the opaque UserId claim
///
/// Mount AFTER CorrelationIdMiddleware so the CorrelationId is already in LogContext.
/// </summary>
public sealed class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    // Paths excluded from request logging (too noisy / no clinical value)
    private static readonly string[] _excludedPrefixes =
    [
        "/health",
        "/metrics",
        "/favicon.ico"
    ];

    // URL segments after which a value is considered a patient / resource ID → redacted
    private static readonly string[] _piiSegmentPrefixes =
    [
        "patients",
        "clinical",
        "prescriptions",
        "appointments",
        "billing",
        "records"
    ];

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next   = next   ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path.Value ?? "/";

        // Skip excluded paths
        if (_excludedPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
        {
            await _next(context);
            return;
        }

        var scrubbedPath  = ScrubPath(path);
        var userId        = GetUserIdClaim(context);
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? context.TraceIdentifier;
        var sw            = Stopwatch.StartNew();

        using (LogContext.PushProperty("ScrubbedPath", scrubbedPath))
        using (LogContext.PushProperty("ActingUserId",  userId))
        {
            _logger.LogInformation(
                "→ {Method} {ScrubbedPath} [User={UserId}]",
                context.Request.Method, scrubbedPath, userId);

            try
            {
                await _next(context);
            }
            finally
            {
                sw.Stop();
                var statusCode = context.Response.StatusCode;
                var level      = statusCode >= 500 ? LogLevel.Error
                               : statusCode >= 400 ? LogLevel.Warning
                               : LogLevel.Information;

                _logger.Log(level,
                    "← {Method} {ScrubbedPath} {StatusCode} {ElapsedMs}ms [CorrelationId={CorrelationId}]",
                    context.Request.Method, scrubbedPath, statusCode,
                    sw.ElapsedMilliseconds, correlationId);
            }
        }
    }

    /// <summary>
    /// Replace resource ID segments that follow known PHI-bearing route prefixes with [ID].
    /// E.g. /api/v1/patients/abc-123/records → /api/v1/patients/[ID]/records
    /// </summary>
    private static string ScrubPath(string path)
    {
        var segments = path.Split('/', StringSplitOptions.None);
        var result   = new List<string>(segments.Length);
        var scrubNext = false;

        foreach (var segment in segments)
        {
            if (scrubNext && !string.IsNullOrEmpty(segment))
            {
                result.Add("[ID]");
                scrubNext = false;
            }
            else
            {
                result.Add(segment);
                scrubNext = _piiSegmentPrefixes.Contains(segment, StringComparer.OrdinalIgnoreCase);
            }
        }

        return string.Join("/", result);
    }

    private static string GetUserIdClaim(HttpContext context)
    {
        var user = context.User;
        if (user?.Identity?.IsAuthenticated != true) return "anonymous";

        // Prefer 'sub' claim (JWT subject = user ID)
        return user.FindFirst("sub")?.Value
            ?? user.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
            ?? "authenticated";
    }
}

public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseEHRRequestLogging(this IApplicationBuilder app)
        => app.UseMiddleware<RequestLoggingMiddleware>();
}

