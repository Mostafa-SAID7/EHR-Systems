using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.Observability.HealthChecks;

/// <summary>
/// Service for orchestrating and aggregating health checks.
/// Single responsibility: Health check coordination.
/// </summary>
public interface IHealthCheckService
{
    /// <summary>
    /// Run all registered health checks and get results.
    /// </summary>
    Task<HealthCheckResult> RunAllChecksAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Get results for specific component.
    /// </summary>
    Task<HealthCheckResult> GetComponentHealthAsync(string componentName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get overall system status.
    /// </summary>
    SystemHealth GetSystemHealth();
}

/// <summary>
/// Result of a health check.
/// </summary>
public class HealthCheckResult
{
    /// <summary>
    /// Is the component healthy.
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Component status message.
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Response time in milliseconds.
    /// </summary>
    public long ResponseTimeMs { get; set; }

    /// <summary>
    /// Error details if unhealthy.
    /// </summary>
    public string? Error { get; set; }

    /// <summary>
    /// Timestamp of check.
    /// </summary>
    public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Overall system health snapshot.
/// </summary>
public class SystemHealth
{
    /// <summary>
    /// Overall system status.
    /// </summary>
    public string Status { get; set; } = null!;

    /// <summary>
    /// System checks results by component.
    /// </summary>
    public Dictionary<string, HealthCheckResult> Components { get; set; } = new();

    /// <summary>
    /// Number of healthy components.
    /// </summary>
    public int HealthyCount => Components.Values.Count(x => x.IsHealthy);

    /// <summary>
    /// Total number of components.
    /// </summary>
    public int TotalCount => Components.Count;

    /// <summary>
    /// Whether all components are healthy.
    /// </summary>
    public bool IsHealthy => HealthyCount == TotalCount;
}
