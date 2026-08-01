using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace EHRPlatform.Observability.Middleware;

/// <summary>
/// Middleware to add correlation ID to all requests for distributed tracing.
/// Single responsibility: Correlation ID injection and propagation.
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

