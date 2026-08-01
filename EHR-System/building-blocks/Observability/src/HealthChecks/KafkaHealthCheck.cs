using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EHRPlatform.Observability.HealthChecks;

/// <summary>
/// Health check for Apache Kafka cluster.
/// Single responsibility: Kafka broker connectivity check.
/// </summary>
public class KafkaHealthCheck : IHealthCheck
{
    private readonly string _bootstrapServers;

    public KafkaHealthCheck(string bootstrapServers)
    {
        _bootstrapServers = bootstrapServers ?? throw new ArgumentNullException(nameof(bootstrapServers));
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var config = new AdminClientConfig 
            { 
                BootstrapServers = _bootstrapServers,
                RequestTimeoutMs = 5000
            };

            using var adminClient = new AdminClientBuilder(config).Build();
            var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(5));

            return metadata?.Brokers?.Count > 0
                ? Task.FromResult(HealthCheckResult.Healthy($"Kafka connection successful ({metadata.Brokers.Count} brokers)"))
                : Task.FromResult(HealthCheckResult.Unhealthy("Kafka: No brokers found"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy($"Kafka health check failed: {ex.Message}", ex));
        }
    }
}
