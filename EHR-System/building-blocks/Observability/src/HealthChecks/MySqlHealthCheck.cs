using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EHRPlatform.Observability.HealthChecks;

/// <summary>
/// Health check for MySQL database.
/// Single responsibility: MySQL connectivity check.
/// </summary>
public class MySqlHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public MySqlHealthCheck(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new MySqlConnector.MySqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy("MySQL connection successful");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"MySQL health check failed: {ex.Message}", ex);
        }
    }
}
