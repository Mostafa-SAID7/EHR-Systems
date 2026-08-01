using System;
using System.Collections.Generic;

namespace EHRPlatform.Observability.Telemetry;

/// <summary>
/// Interface for tracking error rates and error-related metrics.
/// Single responsibility: Error metrics tracking contract.
/// </summary>
public interface IErrorMetrics
{
    /// <summary>
    /// Record an error occurrence.
    /// </summary>
    void RecordError(string errorType, string? message = null, Dictionary<string, object>? context = null);

    /// <summary>
    /// Record exception.
    /// </summary>
    void RecordException(Exception exception, string? operation = null, Dictionary<string, object>? context = null);

    /// <summary>
    /// Get error rate for time period.
    /// </summary>
    double GetErrorRate(TimeSpan timeWindow);

    /// <summary>
    /// Get error count by type.
    /// </summary>
    Dictionary<string, long> GetErrorsByType();
}
