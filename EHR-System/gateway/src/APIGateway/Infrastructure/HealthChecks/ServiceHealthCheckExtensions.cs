using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EHRPlatform.Gateway.Infrastructure.HealthChecks;

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
        var services = configuration.GetSection("Services").Get<Dictionary<string, ServiceConfigDto>>() ?? new();

        foreach (var service in services)
        {
            var serviceName = service.Key;
            var config = service.Value;

            builder.AddCheck(
                $"{serviceName}-health",
                new ServiceHealthCheck(
                    new DefaultHttpClientFactory(),
                    LoggerFactory.Create(b => b.AddConsole()).CreateLogger<ServiceHealthCheck>(),
                    serviceName,
                    $"{config.BaseUrl}/health"),
                tags: new[] { "services", serviceName });
        }

        return builder;
    }
}
