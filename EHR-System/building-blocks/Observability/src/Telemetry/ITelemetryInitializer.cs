using System;

namespace EHRPlatform.Observability.Telemetry;

/// <summary>
/// Interface for telemetry system initialization and configuration.
/// Single responsibility: Telemetry setup contract.
/// </summary>
public interface ITelemetryInitializer
{
    /// <summary>
    /// Initialize OpenTelemetry with tracing, metrics, and logging.
    /// </summary>
    void Initialize();

    /// <summary>
    /// Enable distributed tracing.
    /// </summary>
    void EnableDistributedTracing(string serviceName, string? jaegerEndpoint = null);

    /// <summary>
    /// Enable metrics collection.
    /// </summary>
    void EnableMetrics();

    /// <summary>
    /// Enable logs collection.
    /// </summary>
    void EnableLogging();

    /// <summary>
    /// Add custom trace attributes.
    /// </summary>
    void AddTraceAttribute(string key, object value);

    /// <summary>
    /// Shutdown telemetry gracefully.
    /// </summary>
    void Shutdown();
}
