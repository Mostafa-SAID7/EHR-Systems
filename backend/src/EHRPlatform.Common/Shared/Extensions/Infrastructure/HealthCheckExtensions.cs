#nullable enable

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EHRPlatform.Common.Infrastructure.Health;

/// <summary>
/// Legacy health check extensions - delegates to focused implementations.
/// 
/// Endpoints:
///   - /health         → Overall health (all dependencies)
///   - /health/live    → Liveness (is service running)
///   - /health/ready   → Readiness (dependencies ready)
///
/// Usage:
///   builder.Services.AddComprehensiveHealthChecks(builder.Configuration);
///   app.MapHealthCheckEndpoints();
///
/// Implementation is split across:
/// - HealthCheckRegistrationExtensions.cs: Registration logic
/// - HealthCheckEndpointMappingExtensions.cs: Endpoint mapping
/// - HealthCheckResponseWriters.cs: Response formatting
/// </summary>
public static class HealthChecksExtensions
{
    /// <summary>
    /// Configure comprehensive health checks for all dependencies.
    /// Delegates to HealthCheckRegistrationExtensions.
    /// </summary>
    public static IServiceCollection AddComprehensiveHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
        => services.AddComprehensiveHealthChecks(configuration);

    /// <summary>
    /// Map health check endpoints.
    /// Delegates to HealthCheckEndpointMappingExtensions.
    /// </summary>
    public static IApplicationBuilder MapHealthCheckEndpoints(this IApplicationBuilder app)
        => app.MapHealthCheckEndpoints();
}

