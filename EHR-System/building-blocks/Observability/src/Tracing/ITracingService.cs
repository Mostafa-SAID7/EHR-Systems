using System;
using System.Threading.Tasks;

namespace EHRPlatform.Observability.Tracing;

/// <summary>
/// Interface for distributed tracing service (OpenTelemetry).
/// Single responsibility: Create and manage trace spans.
/// </summary>
public interface ITracingService
{
    /// <summary>
    /// Start a new trace span.
    /// </summary>
    ISpanBuilder StartSpan(string operationName);

    /// <summary>
    /// Get current active span.
    /// </summary>
    ISpan GetActiveSpan();

    /// <summary>
    /// Extract trace context from carrier.
    /// </summary>
    TraceContext ExtractTraceContext(ITraceContextCarrier carrier);

    /// <summary>
    /// Inject trace context into carrier.
    /// </summary>
    void InjectTraceContext(TraceContext context, ITraceContextCarrier carrier);
}
