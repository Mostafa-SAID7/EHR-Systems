using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Mvc;
using EHRPlatform.Gateway.DTOs.Responses;
using EHRPlatform.Gateway.Infrastructure.HealthChecks.Models;

namespace EHRPlatform.Gateway.Controllers;

/// <summary>
/// Health check endpoints for monitoring gateway and downstream services.
/// 
/// Endpoints:
/// - GET /health -> Overall system health
/// - GET /health/live -> Liveness probe for container orchestration
/// - GET /health/ready -> Readiness probe (depends on critical services)
/// - GET /health/detailed -> Detailed service status
/// </summary>
[ApiController]
[Route("health")]
[AllowAnonymous]
public class HealthCheckController : ControllerBase
{
    private readonly IHealthChecksService _healthChecksService;
    private readonly ILogger<HealthCheckController> _logger;

    public HealthCheckController(
        IHealthChecksService healthChecksService,
        ILogger<HealthCheckController> logger)
    {
        _healthChecksService = healthChecksService;
        _logger = logger;
    }

    /// <summary>
    /// Liveness probe - indicates if the gateway process is running.
    /// Container orchestration (K8s) uses this to determine if pod should be restarted.
    /// 
    /// Always returns 200 if gateway is up, regardless of service status.
    /// </summary>
    [HttpGet("live")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult Liveness()
    {
        return Ok(new
        {
            status = "alive",
            timestamp = DateTime.UtcNow,
            uptime = System.Diagnostics.Process.GetCurrentProcess().TotalProcessorTime
        });
    }

    /// <summary>
    /// Readiness probe - indicates if gateway is ready to accept traffic.
    /// Returns 200 only if critical downstream services are healthy.
    /// Returns 503 if any critical service is down.
    /// 
    /// Used by load balancers to route traffic only to ready instances.
    /// </summary>
    [HttpGet("ready")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Readiness()
    {
        var report = await _healthChecksService.CheckHealthAsync();

        var criticalServices = new[] { "identity", "patient", "audit", "appointment" };
        var criticalServiceChecks = report.Entries
            .Where(e => criticalServices.Any(cs => e.Key.Contains(cs)))
            .ToList();

        var allCriticalHealthy = criticalServiceChecks.All(e => e.Value.Status == HealthStatus.Healthy);

        if (!allCriticalHealthy)
        {
            _logger.LogWarning("Readiness check failed - critical services not healthy");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new
            {
                status = "not_ready",
                reason = "One or more critical services are not healthy",
                timestamp = DateTime.UtcNow
            });
        }

        return Ok(new
        {
            status = "ready",
            timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Overall gateway health - comprehensive status of all services.
    /// Returns aggregate health status based on all service checks.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Overall()
    {
        var report = await _healthChecksService.CheckHealthAsync();

        var response = new GatewayHealthCheckResponse
        {
            Status = report.Status.ToString(),
            Timestamp = DateTime.UtcNow,
            Services = report.Entries.Select(e => new ServiceHealthCheckDetail
            {
                Name = e.Key,
                Status = e.Value.Status.ToString(),
                Description = e.Value.Description ?? string.Empty,
                Duration = e.Value.Duration.TotalMilliseconds,
                Data = e.Value.Data.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? string.Empty)
            }).ToList(),
            Summary = new HealthSummary
            {
                Total = report.Entries.Count,
                Healthy = report.Entries.Count(e => e.Value.Status == HealthStatus.Healthy),
                Degraded = report.Entries.Count(e => e.Value.Status == HealthStatus.Degraded),
                Unhealthy = report.Entries.Count(e => e.Value.Status == HealthStatus.Unhealthy)
            }
        };

        var statusCode = report.Status == HealthStatus.Healthy ? 
            StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable;

        return StatusCode(statusCode, response);
    }

    /// <summary>
    /// Detailed gateway health - full breakdown of all service statuses.
    /// Includes response times, error details, and recommendations.
    /// </summary>
    [HttpGet("detailed")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> Detailed()
    {
        var report = await _healthChecksService.CheckHealthAsync();

        var services = new Dictionary<string, ServiceHealthDetail>();

        foreach (var entry in report.Entries)
        {
            var serviceName = entry.Key.Replace("-health", "");

            var detail = new ServiceHealthDetail
            {
                Name = serviceName,
                Status = entry.Value.Status.ToString(),
                Description = entry.Value.Description ?? string.Empty,
                Duration = entry.Value.Duration.TotalMilliseconds,
                LastUpdated = DateTime.UtcNow,
                Data = entry.Value.Data.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? string.Empty),
                Recommendation = GetRecommendation(entry.Value.Status, serviceName)
            };

            services[serviceName] = detail;
        }

        var response = new DetailedGatewayHealthResponse
        {
            Status = report.Status.ToString(),
            Timestamp = DateTime.UtcNow,
            Services = services,
            Summary = new HealthSummary
            {
                Total = report.Entries.Count,
                Healthy = report.Entries.Count(e => e.Value.Status == HealthStatus.Healthy),
                Degraded = report.Entries.Count(e => e.Value.Status == HealthStatus.Degraded),
                Unhealthy = report.Entries.Count(e => e.Value.Status == HealthStatus.Unhealthy)
            },
            IsCritical = IsCriticalServiceDown(report)
        };

        var statusCode = report.Status == HealthStatus.Healthy ?
            StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable;

        return StatusCode(statusCode, response);
    }

    /// <summary>
    /// Service-specific health check.
    /// GET /health/services/{serviceName}
    /// </summary>
    [HttpGet("services/{serviceName}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> ServiceHealth(string serviceName)
    {
        var report = await _healthChecksService.CheckHealthAsync();

        var serviceEntry = report.Entries.FirstOrDefault(e => e.Key.Contains(serviceName));

        if (serviceEntry.Key == null)
        {
            return NotFound(new
            {
                error = $"Service '{serviceName}' not found",
                availableServices = report.Entries.Keys
            });
        }

        var statusCode = serviceEntry.Value.Status == HealthStatus.Healthy ?
            StatusCodes.Status200OK : StatusCodes.Status503ServiceUnavailable;

        return StatusCode(statusCode, new ServiceHealthResponse
        {
            ServiceName = serviceName,
            Status = serviceEntry.Value.Status.ToString(),
            Description = serviceEntry.Value.Description ?? string.Empty,
            Duration = serviceEntry.Value.Duration.TotalMilliseconds,
            Data = serviceEntry.Value.Data.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? string.Empty),
            Timestamp = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Get health metrics suitable for monitoring dashboards.
    /// GET /health/metrics
    /// </summary>
    [HttpGet("metrics")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> Metrics()
    {
        var report = await _healthChecksService.CheckHealthAsync();

        return Ok(new HealthMetrics
        {
            Timestamp = DateTime.UtcNow,
            GatewayStatus = report.Status.ToString(),
            TotalServices = report.Entries.Count,
            HealthyServices = report.Entries.Count(e => e.Value.Status == HealthStatus.Healthy),
            DegradedServices = report.Entries.Count(e => e.Value.Status == HealthStatus.Degraded),
            UnhealthyServices = report.Entries.Count(e => e.Value.Status == HealthStatus.Unhealthy),
            AverageResponseTime = report.Entries.Average(e => e.Value.Duration.TotalMilliseconds),
            MaxResponseTime = report.Entries.Max(e => e.Value.Duration.TotalMilliseconds),
            MinResponseTime = report.Entries.Min(e => e.Value.Duration.TotalMilliseconds),
            HealthPercentage = (double)report.Entries.Count(e => e.Value.Status == HealthStatus.Healthy) / 
                               report.Entries.Count * 100
        });
    }

    private static bool IsCriticalServiceDown(HealthReport report)
    {
        var criticalServices = new[] { "identity", "patient", "audit", "appointment" };
        
        return report.Entries
            .Where(e => criticalServices.Any(cs => e.Key.Contains(cs)))
            .Any(e => e.Value.Status == HealthStatus.Unhealthy);
    }

    private static string GetRecommendation(HealthStatus status, string serviceName)
    {
        return status switch
        {
            HealthStatus.Healthy => $"{serviceName} is operating normally",
            HealthStatus.Degraded => $"{serviceName} is responding slowly - check logs for performance issues",
            HealthStatus.Unhealthy => $"{serviceName} is down - restart service or check service logs for errors",
            _ => "Unknown health status"
        };
    }
}
