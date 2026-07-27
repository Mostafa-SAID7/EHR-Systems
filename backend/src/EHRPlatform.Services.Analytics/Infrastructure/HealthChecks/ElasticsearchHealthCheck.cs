using Elastic.Clients.Elasticsearch;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EHRPlatform.Services.Analytics.Infrastructure.HealthChecks;

public class ElasticsearchHealthCheck : IHealthCheck
{
    private readonly ElasticsearchClient? _client;
    private readonly ILogger<ElasticsearchHealthCheck> _logger;

    public ElasticsearchHealthCheck(ElasticsearchClient? client, ILogger<ElasticsearchHealthCheck> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (_client == null)
            return HealthCheckResult.Healthy("Elasticsearch not configured");

        try
        {
            var response = await _client.PingAsync(cancellationToken);
            if (response.IsValidResponse)
            {
                _logger.LogDebug("Elasticsearch health check passed");
                return HealthCheckResult.Healthy("Elasticsearch is responding");
            }

            _logger.LogWarning("Elasticsearch health check failed: {DebugInfo}", response.DebugInformation);
            return HealthCheckResult.Unhealthy("Elasticsearch not responding properly");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Elasticsearch health check exception");
            return HealthCheckResult.Unhealthy("Elasticsearch unavailable", ex);
        }
    }
}
