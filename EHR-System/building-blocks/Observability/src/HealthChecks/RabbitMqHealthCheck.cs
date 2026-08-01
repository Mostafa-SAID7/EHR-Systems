using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EHRPlatform.Observability.HealthChecks;

/// <summary>
/// Health check for RabbitMQ message broker.
/// Single responsibility: RabbitMQ connectivity check.
/// </summary>
public class RabbitMqHealthCheck : IHealthCheck
{
    private readonly string _hostName;
    private readonly int _port;

    public RabbitMqHealthCheck(string hostName, int port = 5672)
    {
        _hostName = hostName ?? throw new ArgumentNullException(nameof(hostName));
        _port = port;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var factory = new RabbitMQ.Client.ConnectionFactory { HostName = _hostName, Port = _port };
            using var connection = factory.CreateConnection();
            connection.Close();
            
            return Task.FromResult(HealthCheckResult.Healthy("RabbitMQ connection successful"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy($"RabbitMQ health check failed: {ex.Message}", ex));
        }
    }
}
