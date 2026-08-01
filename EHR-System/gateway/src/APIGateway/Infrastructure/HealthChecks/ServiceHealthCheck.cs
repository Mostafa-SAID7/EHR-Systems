using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;

namespace EHRPlatform.Gateway.Infrastructure.HealthChecks;

/// <summary>
/// Health check implementation for verifying downstream microservices are responding.
/// Used to determine if a service is healthy, degraded, or unhealthy.
/// </summary>
public class ServiceHealthCheck : IHealthCheck
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<ServiceHealthCheck> _logger;
    private readonly string _serviceName;
    private readonly string _healthEndpoint;

    public ServiceHealthCheck(
        IHttpClientFactory httpClientFactory,
        ILogger<ServiceHealthCheck> logger,
        string serviceName,
        string healthEndpoint)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
        _serviceName = serviceName;
        _healthEndpoint = healthEndpoint;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var stopwatch = Stopwatch.StartNew();

        try
        {
            var httpClient = _httpClientFactory.CreateClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            var response = await httpClient.GetAsync(_healthEndpoint, cancellationToken);

            stopwatch.Stop();
            var responseTime = stopwatch.ElapsedMilliseconds;

            var data = new Dictionary<string, object>
            {
                { "ServiceName", _serviceName },
                { "Endpoint", _healthEndpoint },
                { "ResponseTime", $"{responseTime}ms" },
                { "StatusCode", (int)response.StatusCode }
            };

            if (response.IsSuccessStatusCode)
            {
                _logger.LogInformation(
                    "Service {ServiceName} health check passed. Response time: {ResponseTime}ms",
                    _serviceName,
                    responseTime);

                if (responseTime > 1000)
                {
                    return HealthCheckResult.Degraded(
                        $"Service {_serviceName} is slow (response time: {responseTime}ms)",
                        data: data);
                }

                return HealthCheckResult.Healthy(
                    $"Service {_serviceName} is healthy",
                    data: data);
            }

            _logger.LogWarning(
                "Service {ServiceName} health check failed with status {StatusCode}. Response time: {ResponseTime}ms",
                _serviceName,
                response.StatusCode,
                responseTime);

            data.Add("ResponseContent", response.Content.Headers.ContentLength ?? 0);

            return HealthCheckResult.Unhealthy(
                $"Service {_serviceName} returned status {response.StatusCode}",
                data: data);
        }
        catch (HttpRequestException ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                "Service {ServiceName} health check failed with network error: {Error}",
                _serviceName,
                ex.Message);

            return HealthCheckResult.Unhealthy(
                $"Service {_serviceName} is unreachable - {ex.Message}",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "ServiceName", _serviceName },
                    { "Endpoint", _healthEndpoint },
                    { "ErrorType", "NetworkError" },
                    { "ErrorMessage", ex.Message }
                });
        }
        catch (OperationCanceledException ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                "Service {ServiceName} health check timed out after {Timeout}ms",
                _serviceName,
                stopwatch.ElapsedMilliseconds);

            return HealthCheckResult.Unhealthy(
                $"Service {_serviceName} timed out after {stopwatch.ElapsedMilliseconds}ms",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "ServiceName", _serviceName },
                    { "Endpoint", _healthEndpoint },
                    { "ErrorType", "Timeout" },
                    { "TimeoutMs", stopwatch.ElapsedMilliseconds }
                });
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(
                "Service {ServiceName} health check failed with unexpected error: {Error}",
                _serviceName,
                ex.Message);

            return HealthCheckResult.Unhealthy(
                $"Service {_serviceName} health check failed - {ex.Message}",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "ServiceName", _serviceName },
                    { "Endpoint", _healthEndpoint },
                    { "ErrorType", "UnexpectedError" },
                    { "ErrorMessage", ex.Message }
                });
        }
    }
}
