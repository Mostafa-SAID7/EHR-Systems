using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EHRPlatform.Observability.Middleware;

/// <summary>
/// Middleware to add correlation ID to all requests for distributed tracing.
/// Correlation ID is unique per request and flows through all microservices.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Try to get correlation ID from request headers
        var correlationId = context.Request.Headers[CorrelationIdHeader].ToString();

        if (string.IsNullOrEmpty(correlationId))
        {
            // Generate new correlation ID if not provided
            correlationId = Guid.NewGuid().ToString();
        }

        // Add to response headers for client reference
        context.Response.Headers[CorrelationIdHeader] = correlationId;

        // Store in HttpContext items for access in application code
        context.Items[CorrelationIdHeader] = correlationId;

        // Set as trace ID for OpenTelemetry
        System.Diagnostics.Activity.Current?.SetTag("correlationId", correlationId);

        await _next(context);
    }
}

/// <summary>
/// Extension methods for correlation ID middleware.
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    /// <summary>
    /// Add correlation ID middleware to pipeline.
    /// </summary>
    public static IApplicationBuilder UseCorrelationId(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CorrelationIdMiddleware>();
    }

    /// <summary>
    /// Get correlation ID from HTTP context.
    /// </summary>
    public static string? GetCorrelationId(this HttpContext context)
    {
        return context.Items["X-Correlation-ID"] as string;
    }

    /// <summary>
    /// Get correlation ID from HTTP context items (for use in application code).
    /// </summary>
    public static string? GetCorrelationIdFromContext(this HttpContext? context)
    {
        return context?.Items.TryGetValue("X-Correlation-ID", out var correlationId) == true
            ? correlationId as string
            : null;
    }
}
