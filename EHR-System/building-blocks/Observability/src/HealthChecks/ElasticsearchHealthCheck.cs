using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EHRPlatform.Observability.HealthChecks;

/// <summary>
/// Health check for Elasticsearch cluster.
/// Single responsibility: Elasticsearch connectivity check.
/// </summary>
public class ElasticsearchHealthCheck : IHealthCheck
{
    private readonly string _endpoint;

    public ElasticsearchHealthCheck(string endpoint)
    {
        _endpoint = endpoint ?? throw new ArgumentNullException(nameof(endpoint));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var settings = new Elasticsearch.Net.ConnectionConfiguration(new Uri(_endpoint));
            var lowLevelClient = new Elasticsearch.Net.ElasticLowLevelClient(settings);
            
            var response = await lowLevelClient.PingAsync<Elasticsearch.Net.BytesResponse>(cancellationToken: cancellationToken);
            
            return response.ApiCall.HasSuccessfulStatusCode
                ? HealthCheckResult.Healthy("Elasticsearch connection successful")
                : HealthCheckResult.Unhealthy($"Elasticsearch health check failed: {response.ApiCall.HttpStatusCode}");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Elasticsearch health check failed: {ex.Message}", ex);
        }
    }
}
