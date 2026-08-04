using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace EHRPlatform.Observability.Telemetry;

/// <summary>
/// Implementation of performance monitoring using .NET Diagnostics
/// </summary>
public class PerformanceMonitor : IPerformanceMonitor
{
    private readonly ApplicationMetrics _metrics;

    public PerformanceMonitor(ApplicationMetrics metrics)
    {
        _metrics = metrics;
    }

    public IDisposable MeasureOperation(string operationName, Dictionary<string, object>? tags = null)
    {
        var activity = ApplicationMetrics.StartActivity(operationName);
        if (tags != null)
        {
            foreach (var tag in tags)
            {
                activity?.SetTag(tag.Key, tag.Value);
            }
        }
        return activity ?? new NoOpDisposable();
    }

    public void RecordGauge(string metricName, double value, Dictionary<string, object>? tags = null)
    {
        // Gauge metrics are typically sent to external monitoring systems (Prometheus, DataDog, etc.)
        // For now, log the gauge value
    }

    public void IncrementCounter(string metricName, long value = 1, Dictionary<string, object>? tags = null)
    {
        // Counter increments are tracked via ApplicationMetrics
        _metrics.IncrementCounter(metricName, value);
    }

    public void RecordHistogram(string metricName, long value, Dictionary<string, object>? tags = null)
    {
        // Histogram metrics are typically sent to external monitoring systems
    }

    private class NoOpDisposable : IDisposable
    {
        public void Dispose() { }
    }
}
