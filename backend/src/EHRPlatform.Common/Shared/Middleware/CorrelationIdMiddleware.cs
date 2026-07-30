#nullable enable

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Serilog.Context;

namespace EHRPlatform.Common.Shared.Middleware;

/// <summary>
/// Generates or propagates an X-Correlation-ID header for every request.
///
/// Priority order:
///   1. Incoming X-Correlation-ID header (propagated from client / upstream gateway)
///   2. Incoming X-Request-ID header (legacy clients)
///   3. New GUID generated here
///
/// The correlation ID is:
///   - Stored in HttpContext.Items["CorrelationId"] for downstream code
///   - Echoed in the response header X-Correlation-ID
///   - Pushed into Serilog LogContext so every log line includes it
///   - Added to the HttpContext TraceIdentifier for ASP.NET diagnostics
///
/// HIPAA: correlation IDs must never include patient identifiers.
/// </summary>
public sealed class CorrelationIdMiddleware
{
    private const string CorrelationIdHeaderName  = "X-Correlation-ID";
    private const string RequestIdHeaderName       = "X-Request-ID";
    private const string ContextItemKey            = "CorrelationId";

    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next   = next   ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);

        // Store for downstream services
        context.Items[ContextItemKey]      = correlationId;
        context.TraceIdentifier             = correlationId;

        // Echo back in response
        context.Response.OnStarting(() =>
        {
            context.Response.Headers[CorrelationIdHeaderName] = correlationId;
            return Task.CompletedTask;
        });

        // Push into Serilog context — all log entries in this request include it
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            _logger.LogDebug("Correlation ID resolved: {CorrelationId}", correlationId);
            await _next(context);
        }
    }

    private static string ResolveCorrelationId(HttpContext context)
    {
        if (context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out var fromHeader)
            && !string.IsNullOrWhiteSpace(fromHeader))
            return fromHeader.ToString();

        if (context.Request.Headers.TryGetValue(RequestIdHeaderName, out var fromRequestId)
            && !string.IsNullOrWhiteSpace(fromRequestId))
            return fromRequestId.ToString();

        return Guid.NewGuid().ToString("D");
    }
}

/// <summary>Extension methods to register CorrelationIdMiddleware.</summary>
public static class CorrelationIdMiddlewareExtensions
{
    public static IApplicationBuilder UseEHRCorrelationId(this IApplicationBuilder app)
        => app.UseMiddleware<CorrelationIdMiddleware>();
}

