namespace EHRPlatform.Gateway.Infrastructure.Middleware;

/// <summary>
/// Middleware to add correlation ID to every request for distributed tracing.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeader = "X-Correlation-ID";
    private const string TraceIdHeader = "X-Trace-ID";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Get or create correlation ID
        var correlationId = context.Request.Headers.TryGetValue(CorrelationIdHeader, out var value)
            ? value.ToString()
            : Guid.NewGuid().ToString();

        var traceId = context.TraceIdentifier;

        // Add to context
        context.Items[CorrelationIdHeader] = correlationId;
        context.Items[TraceIdHeader] = traceId;

        // Add to response headers
        context.Response.Headers.Add(CorrelationIdHeader, correlationId);
        context.Response.Headers.Add(TraceIdHeader, traceId);

        // Log with correlation ID
        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        using (Serilog.Context.LogContext.PushProperty("TraceId", traceId))
        {
            await _next(context);
        }
    }
}
