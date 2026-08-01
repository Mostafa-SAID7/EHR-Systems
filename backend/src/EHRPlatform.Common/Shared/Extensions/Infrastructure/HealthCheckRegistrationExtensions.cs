#nullable enable

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EHRPlatform.Common.Infrastructure.Health;

/// <summary>
/// Health check registration extensions.
/// Single responsibility: Register all health checks only.
/// </summary>
public static class HealthCheckRegistrationExtensions
{
    /// <summary>
    /// Configure comprehensive health checks for all dependencies.
    /// Registers: Cache, MongoDB, Elasticsearch checks.
    /// 
    /// Must be called in Program.cs before build().
    /// Usage: builder.Services.AddComprehensiveHealthChecks(builder.Configuration);
    /// </summary>
    public static IServiceCollection AddComprehensiveHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var builder = services.AddHealthChecks();

        // Register all individual health checks via their extensions
        builder.AddCacheHealthCheck();
        builder.AddMongoHealthCheck();
        builder.AddElasticsearchHealthCheck();

        return services;
    }
}
