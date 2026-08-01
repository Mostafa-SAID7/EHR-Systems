using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EHRPlatform.Observability.HealthChecks;

/// <summary>
/// Generic health check for external services/APIs.
/// Single responsibility: External HTTP service connectivity check.
/// </summary>
public class ServiceHealthCheck : IHealthCheck
{
    private readonly HttpClient _httpClient;
    private readonly string _serviceName;
    private readonly string _healthCheckUrl;
    private readonly int _timeoutSeconds;

    public ServiceHealthCheck(HttpClient httpClient, string serviceName, string healthCheckUrl, int timeoutSeconds = 5)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _serviceName = serviceName ?? throw new ArgumentNullException(nameof(serviceName));
        _healthCheckUrl = healthCheckUrl ?? throw new ArgumentNullException(nameof(healthCheckUrl));
        _timeoutSeconds = timeoutSeconds;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            cts.CancelAfter(TimeSpan.FromSeconds(_timeoutSeconds));

            var response = await _httpClient.GetAsync(_healthCheckUrl, HttpCompletionOption.ResponseHeadersRead, cts.Token);

            return response.IsSuccessStatusCode
                ? HealthCheckResult.Healthy($"{_serviceName} service is healthy")
                : HealthCheckResult.Unhealthy($"{_serviceName} returned {response.StatusCode}");
        }
        catch (OperationCanceledException)
        {
            return HealthCheckResult.Unhealthy($"{_serviceName} health check timed out after {_timeoutSeconds}s");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"{_serviceName} health check failed: {ex.Message}", ex);
        }
    }
}
