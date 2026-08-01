using System;
using System.Threading.Tasks;

namespace EHRPlatform.Observability.Tracing;

/// <summary>
/// Active trace span.
/// Single responsibility: Represent and manage trace span lifecycle.
/// </summary>
public interface ISpan : IDisposable, IAsyncDisposable
{
    /// <summary>
    /// Span ID.
    /// </summary>
    string SpanId { get; }

    /// <summary>
    /// Trace ID.
    /// </summary>
    string TraceId { get; }

    /// <summary>
    /// Operation name.
    /// </summary>
    string OperationName { get; set; }

    /// <summary>
    /// Set span status.
    /// </summary>
    void SetStatus(SpanStatus status, string? description = null);

    /// <summary>
    /// Record exception.
    /// </summary>
    void RecordException(Exception exception);

    /// <summary>
    /// Finish span.
    /// </summary>
    Task FinishAsync();
}
