using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace EHRPlatform.Observability.Middleware;

/// <summary>
/// Extension methods for correlation ID middleware and context.
/// Single responsibility: Providing API extensions for correlation ID access.
/// </summary>
public static class CorrelationIdMiddlewareExtensions
{
    private const string CorrelationIdHeader = "X-Correlation-ID";

    /// <summary>
    /// Add correlation ID middleware to ASP.NET Core pipeline.
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
        return context.Items[CorrelationIdHeader] as string;
    }

    /// <summary>
    /// Get correlation ID from HTTP context (nullable context version).
    /// </summary>
    public static string? GetCorrelationIdFromContext(this HttpContext? context)
    {
        return context?.Items.TryGetValue(CorrelationIdHeader, out var correlationId) == true
            ? correlationId as string
            : null;
    }
}
