namespace EHRPlatform.Observability.Tracing;

/// <summary>
/// Trace context carrier for injection/extraction.
/// Single responsibility: Carry trace context across boundaries.
/// </summary>
public interface ITraceContextCarrier
{
    /// <summary>
    /// Get carrier field value.
    /// </summary>
    string? Get(string key);

    /// <summary>
    /// Set carrier field value.
    /// </summary>
    void Set(string key, string value);
}
