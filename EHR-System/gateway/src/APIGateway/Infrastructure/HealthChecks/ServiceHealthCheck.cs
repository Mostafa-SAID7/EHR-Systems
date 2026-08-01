using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Diagnostics;
using System.Net.Http.Json;

namespace EHRPlatform.Gateway.Infrastructure.HealthChecks
{
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

                // Call service health endpoint
                var response = await httpClient.GetAsync(_healthEndpoint, cancellationToken);

                stopwatch.Stop();
                var responseTime = stopwatch.ElapsedMilliseconds;

                // Determine health based on response
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

                    // Degraded if slow response (>1000ms)
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

                // Service returned non-success status code
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

    /// <summary>
    /// Factory for creating health checks for all downstream services.
    /// Registered in dependency injection container.
    /// </summary>
    public static class ServiceHealthCheckExtensions
    {
        public static IHealthChecksBuilder AddServiceHealthChecks(
            this IHealthChecksBuilder builder,
            IConfiguration configuration)
        {
            var services = configuration.GetSection("Services").Get<Dictionary<string, ServiceConfig>>() ?? new();

            foreach (var service in services)
            {
                var serviceName = service.Key;
                var config = service.Value;

                // Create a health check for this service
                builder.AddCheck(
                    $"{serviceName}-health",
                    new ServiceHealthCheck(
                        new DefaultHttpClientFactory(), // Will be replaced with DI version
                        LoggerFactory.Create(b => b.AddConsole()).CreateLogger<ServiceHealthCheck>(),
                        serviceName,
                        $"{config.BaseUrl}/health"),
                    tags: new[] { "services", serviceName });
            }

            return builder;
        }
    }

    /// <summary>
    /// Service configuration for health check endpoint.
    /// </summary>
    public class ServiceConfig
    {
        public string Name { get; set; } = string.Empty;
        public string BaseUrl { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Version { get; set; } = "1.0";
        public bool Critical { get; set; } = true; // If false, system can operate without this service
    }

    /// <summary>
    /// Default HTTP client factory for health check dependencies.
    /// </summary>
    public class DefaultHttpClientFactory : IHttpClientFactory
    {
        public HttpClient CreateClient(string name)
        {
            return new HttpClient();
        }
    }

    /// <summary>
    /// Health check response aggregator - combines all service checks into single response.
    /// </summary>
    public class HealthCheckResponseAggregator
    {
        public static async Task<GatewayHealthResponse> AggregateAsync(
            IHealthChecksService healthChecksService,
            HealthReport report)
        {
            var services = new Dictionary<string, ServiceHealthStatus>();

            foreach (var entry in report.Entries)
            {
                var parts = entry.Key.Split('-');
                var serviceName = parts[0];

                services[serviceName] = new ServiceHealthStatus
                {
                    Name = serviceName,
                    Status = entry.Value.Status.ToString(),
                    ResponseTime = entry.Value.Data.TryGetValue("ResponseTime", out var time) ? time.ToString() : "N/A",
                    Description = entry.Value.Description ?? "No description",
                    LastChecked = DateTime.UtcNow
                };
            }

            return new GatewayHealthResponse
            {
                OverallStatus = report.Status.ToString(),
                Timestamp = DateTime.UtcNow,
                Services = services,
                TotalServices = services.Count,
                HealthyServices = services.Count(s => s.Value.Status == "Healthy"),
                DegradedServices = services.Count(s => s.Value.Status == "Degraded"),
                UnhealthyServices = services.Count(s => s.Value.Status == "Unhealthy")
            };
        }
    }

    /// <summary>
    /// Gateway health response model.
    /// </summary>
    public class GatewayHealthResponse
    {
        public string OverallStatus { get; set; } = "Unknown";
        public DateTime Timestamp { get; set; }
        public Dictionary<string, ServiceHealthStatus> Services { get; set; } = new();
        public int TotalServices { get; set; }
        public int HealthyServices { get; set; }
        public int DegradedServices { get; set; }
        public int UnhealthyServices { get; set; }
    }

    /// <summary>
    /// Individual service health status.
    /// </summary>
    public class ServiceHealthStatus
    {
        public string Name { get; set; } = string.Empty;
        public string Status { get; set; } = "Unknown";
        public string ResponseTime { get; set; } = "N/A";
        public string Description { get; set; } = string.Empty;
        public DateTime LastChecked { get; set; }
    }
}
