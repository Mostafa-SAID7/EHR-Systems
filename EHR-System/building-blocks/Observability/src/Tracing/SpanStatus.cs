namespace EHRPlatform.Observability.Tracing;

/// <summary>
/// Span status enumeration.
/// Single responsibility: Trace span status values.
/// </summary>
public enum SpanStatus
{
    /// <summary>
    /// Operation completed successfully.
    /// </summary>
    Ok = 0,

    /// <summary>
    /// Operation failed.
    /// </summary>
    Error = 1,

    /// <summary>
    /// Status unset.
    /// </summary>
    Unset = 2
}
