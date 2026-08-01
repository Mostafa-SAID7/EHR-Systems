using System;
using System.Collections.Generic;

namespace EHRPlatform.Observability.Telemetry;

/// <summary>
/// Interface for performance monitoring and metrics tracking.
/// Single responsibility: Performance metrics contract.
/// </summary>
public interface IPerformanceMonitor
{
    /// <summary>
    /// Measure operation duration.
    /// </summary>
    IDisposable MeasureOperation(string operationName, Dictionary<string, object>? tags = null);

    /// <summary>
    /// Record custom gauge metric.
    /// </summary>
    void RecordGauge(string metricName, double value, Dictionary<string, object>? tags = null);

    /// <summary>
    /// Increment counter metric.
    /// </summary>
    void IncrementCounter(string metricName, long value = 1, Dictionary<string, object>? tags = null);

    /// <summary>
    /// Record histogram/distribution metric.
    /// </summary>
    void RecordHistogram(string metricName, long value, Dictionary<string, object>? tags = null);
}
