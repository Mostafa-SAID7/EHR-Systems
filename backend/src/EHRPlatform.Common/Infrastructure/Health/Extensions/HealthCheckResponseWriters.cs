#nullable enable

using Microsoft.AspNetCore.Http;
using System.Net.Mime;
using System.Text.Json;
using EHRPlatform.Common.Shared.Utilities.Helpers;

namespace EHRPlatform.Common.Infrastructure.Health;

/// <summary>
/// Health check response writers for formatting health check results.
/// Single responsibility: Write formatted responses only.
/// </summary>
public static class HealthCheckResponseWriters
{
    /// <summary>
    /// Simple health response for liveness probe (/health/live).
    /// Returns 200 if service is running, 503 if not.
    /// Minimal response: status + timestamp only.
    /// </summary>
    public static async Task WriteSimpleHealthResponse(
        HttpContext context,
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport report)
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;
        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTimeHelper.UtcNow
        };
        await context.Response.WriteAsJsonAsync(response);
    }

    /// <summary>
    /// Detailed health response for readiness probe (/health/ready).
    /// Returns detailed status of all dependencies with check details.
    /// </summary>
    public static async Task WriteDetailedHealthResponse(
        HttpContext context,
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport report)
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTimeHelper.UtcNow,
            duration = report.TotalDuration.TotalMilliseconds,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            })
        };
        await context.Response.WriteAsJsonAsync(response, jsonOptions);
    }

    /// <summary>
    /// Comprehensive health response for full check (/health).
    /// Returns all checks with full data and detailed metrics.
    /// </summary>
    public static async Task WriteComprehensiveHealthResponse(
        HttpContext context,
        Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport report)
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTimeHelper.UtcNow,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds,
                data = e.Value.Data
            })
        };
        await context.Response.WriteAsJsonAsync(response, jsonOptions);
    }
}
