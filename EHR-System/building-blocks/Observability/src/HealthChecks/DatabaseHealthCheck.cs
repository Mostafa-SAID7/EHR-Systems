using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EHRPlatform.Observability.HealthChecks;

/// <summary>
/// Generic health check for database connectivity.
/// Single responsibility: Generic database health check.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly Func<CancellationToken, Task<bool>> _checkAsync;

    public DatabaseHealthCheck(Func<CancellationToken, Task<bool>> checkAsync)
    {
        _checkAsync = checkAsync ?? throw new ArgumentNullException(nameof(checkAsync));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var isHealthy = await _checkAsync(cancellationToken);
            return isHealthy
                ? HealthCheckResult.Healthy("Database connection successful")
                : HealthCheckResult.Unhealthy("Database query returned unexpected result");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Database health check failed: {ex.Message}", ex);
        }
    }
}
