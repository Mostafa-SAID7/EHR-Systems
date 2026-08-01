namespace EHRPlatform.Observability.Tracing;

/// <summary>
/// Trace context for distributed tracing.
/// Single responsibility: Trace context data structure.
/// </summary>
public class TraceContext
{
    /// <summary>
    /// Trace ID.
    /// </summary>
    public string TraceId { get; set; } = null!;

    /// <summary>
    /// Span ID.
    /// </summary>
    public string SpanId { get; set; } = null!;

    /// <summary>
    /// Parent span ID.
    /// </summary>
    public string? ParentSpanId { get; set; }

    /// <summary>
    /// Trace flags (sampled, etc.).
    /// </summary>
    public byte TraceFlags { get; set; }
}
