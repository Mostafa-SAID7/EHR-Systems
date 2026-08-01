using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.Observability.HealthChecks;

/// <summary>
/// Service for orchestrating and aggregating health checks.
/// Single responsibility: Health check coordination interface.
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
