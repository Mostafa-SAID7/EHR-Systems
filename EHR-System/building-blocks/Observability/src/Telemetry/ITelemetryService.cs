using System;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.Observability.Telemetry;

/// <summary>
/// Interface for telemetry service.
/// Single responsibility: Collect and report telemetry data.
/// </summary>
public interface ITelemetryService
{
    /// <summary>
    /// Record a metric.
    /// </summary>
    Task RecordMetricAsync(string metricName, double value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Record a counter increment.
    /// </summary>
    Task IncrementCounterAsync(string counterName, long increment = 1, CancellationToken cancellationToken = default);

    /// <summary>
    /// Record a gauge value.
    /// </summary>
    Task RecordGaugeAsync(string gaugeName, double value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Record a histogram value.
    /// </summary>
    Task RecordHistogramAsync(string histogramName, double value, CancellationToken cancellationToken = default);

    /// <summary>
    /// Start a timer for performance measurement.
    /// </summary>
    IDisposable StartTimer(string timerName);

    /// <summary>
    /// Flush all pending telemetry.
    /// </summary>
    Task FlushAsync(CancellationToken cancellationToken = default);
}
