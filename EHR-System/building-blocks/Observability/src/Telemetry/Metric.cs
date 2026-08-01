using System;

namespace EHRPlatform.Observability.Telemetry;

/// <summary>
/// Metric data structure.
/// Single responsibility: Metric information.
/// </summary>
public class Metric
{
    /// <summary>
    /// Metric name.
    /// </summary>
    public string Name { get; set; } = null!;

    /// <summary>
    /// Metric type (counter, gauge, histogram, etc).
    /// </summary>
    public string Type { get; set; } = null!;

    /// <summary>
    /// Metric value.
    /// </summary>
    public double Value { get; set; }

    /// <summary>
    /// Metric timestamp.
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// Tags/labels for the metric.
    /// </summary>
    public Dictionary<string, string>? Tags { get; set; }
}
