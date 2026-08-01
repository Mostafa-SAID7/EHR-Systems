using Microsoft.Extensions.Diagnostics.HealthChecks;
using EHRPlatform.Gateway.Infrastructure.HealthChecks.Models;

namespace EHRPlatform.Gateway.Infrastructure.HealthChecks;

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
