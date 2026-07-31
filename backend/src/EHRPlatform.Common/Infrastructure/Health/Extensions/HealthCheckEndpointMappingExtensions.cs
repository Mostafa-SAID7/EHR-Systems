#nullable enable

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

namespace EHRPlatform.Common.Infrastructure.Health;

/// <summary>
/// Health check endpoint mapping extensions.
/// Maps three health check endpoints with proper formatting.
/// Single responsibility: Map endpoints only.
/// </summary>
public static class HealthCheckEndpointMappingExtensions
{
    /// <summary>
    /// Map health check endpoints with proper formatting:
    ///   - /health         → All checks (overall status) - comprehensive response
    ///   - /health/live    → Liveness probe (is service running) - simple response
    ///   - /health/ready   → Readiness probe (dependencies ready) - detailed response
    ///
    /// Usage in Program.cs:
    ///   app.MapHealthCheckEndpoints();
    /// </summary>
    public static IApplicationBuilder MapHealthCheckEndpoints(this IApplicationBuilder app)
    {
        app.UseEndpoints(endpoints =>
        {
            // ── /health endpoint (all checks with full details)
            endpoints.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = HealthCheckResponseWriters.WriteComprehensiveHealthResponse
            });

            // ── /health/live endpoint (liveness probe - minimal checks)
            endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = (_) => false,  // Don't check dependencies - just return if service is running
                ResponseWriter = HealthCheckResponseWriters.WriteSimpleHealthResponse
            });

            // ── /health/ready endpoint (readiness probe - only ready checks)
            endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = (check) => check.Tags.Contains("ready"),  // Only checks tagged "ready"
                ResponseWriter = HealthCheckResponseWriters.WriteDetailedHealthResponse
            });
        });

        return app;
    }
}
