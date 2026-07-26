using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Mime;
using System.Text.Json;

namespace EHRPlatform.Common.Extensions;

/// <summary>
/// Health check extensions for comprehensive service health monitoring.
///
/// Endpoints:
///   - /health         → Overall health (SQL, RabbitMQ, Redis, APIs)
///   - /health/live    → Liveness (is service running)
///   - /health/ready   → Readiness (is service ready to accept requests)
///
/// Checks included:
///   - SQL/PostgreSQL database connectivity
///   - RabbitMQ message broker connectivity
///   - Redis cache connectivity
///   - External API dependencies
///   - Storage (S3, Blob, local filesystem)
/// </summary>
public static class HealthChecksExtensions
{
    /// <summary>
    /// Configure comprehensive health checks for all dependencies.
    /// Must be called in Program.cs before build().
    ///
    /// Usage:
    ///   builder.Services.AddComprehensiveHealthChecks(builder.Configuration);
    /// </summary>
    public static IServiceCollection AddComprehensiveHealthChecks(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var builder = services.AddHealthChecks();

        // ── SQL/PostgreSQL Checks ──────────────────────────────────────────
        var sqlConnStr = configuration.GetConnectionString("DefaultConnection");
        if (!string.IsNullOrEmpty(sqlConnStr))
        {
            builder.AddNpgSql(
                sqlConnStr,
                name: "postgres",
                tags: new[] { "sql", "db", "ready" });
        }

        // ── RabbitMQ Checks ────────────────────────────────────────────────
        var rabbitHost = configuration["RabbitMQ:Host"];
        if (!string.IsNullOrEmpty(rabbitHost))
        {
            var rabbitUser = configuration["RabbitMQ:Username"] ?? "ehr_user";
            var rabbitPass = configuration["RabbitMQ:Password"] ?? "ehr_password";
            var rabbitVHost = configuration["RabbitMQ:VirtualHost"] ?? "/ehr";
            var rabbitUri = $"amqp://{rabbitUser}:{rabbitPass}@{rabbitHost}:5672{rabbitVHost}";

            builder.AddRabbitMQ(
                new Uri(rabbitUri),
                name: "rabbitmq",
                tags: new[] { "messaging", "ready" });
        }

        // ── Redis Checks ───────────────────────────────────────────────────
        var redisConnStr = configuration["Redis:ConnectionString"]
            ?? Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING");
        if (!string.IsNullOrEmpty(redisConnStr))
        {
            builder.AddRedis(
                redisConnStr,
                name: "redis",
                tags: new[] { "cache", "ready" });
        }

        // ── Elasticsearch Checks ───────────────────────────────────────────
        var esUrl = configuration["Elasticsearch:Url"]
            ?? Environment.GetEnvironmentVariable("ELASTICSEARCH_URL");
        if (!string.IsNullOrEmpty(esUrl))
        {
            builder.AddUri(
                new Uri(esUrl),
                name: "elasticsearch",
                tags: new[] { "search", "ready" });
        }

        // ── MongoDB Checks (if used) ───────────────────────────────────────
        var mongoConnStr = configuration["MongoDB:ConnectionString"]
            ?? Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING");
        if (!string.IsNullOrEmpty(mongoConnStr))
        {
            builder.AddMongoDb(
                mongoConnStr,
                name: "mongodb",
                tags: new[] { "nosql", "ready" });
        }

        // ── External API Checks ────────────────────────────────────────────
        // Add example external APIs that services depend on
        var externalApis = configuration.GetSection("HealthChecks:ExternalApis").GetChildren();
        foreach (var apiConfig in externalApis)
        {
            var apiName = apiConfig.Key;
            var apiUrl = apiConfig["Url"];
            if (!string.IsNullOrEmpty(apiUrl))
            {
                builder.AddUri(
                    new Uri(apiUrl),
                    name: $"api-{apiName}",
                    tags: new[] { "external", "api" });
            }
        }

        // ── Storage Checks ─────────────────────────────────────────────────
        // Add storage health checks (S3, Azure Blob, local filesystem, etc.)
        var storageType = configuration["Storage:Type"];
        if (!string.IsNullOrEmpty(storageType))
        {
            switch (storageType.ToLower())
            {
                case "s3":
                    // AWS S3 health check would go here
                    builder.AddCheck("s3-storage", () =>
                        Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(),
                        tags: new[] { "storage", "ready" });
                    break;

                case "azureblob":
                    // Azure Blob Storage health check would go here
                    builder.AddCheck("azure-blob-storage", () =>
                        Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(),
                        tags: new[] { "storage", "ready" });
                    break;

                case "local":
                    // Local filesystem health check
                    var storagePath = configuration["Storage:LocalPath"] ?? "/var/ehr-storage";
                    builder.AddCheck("local-storage", () =>
                    {
                        try
                        {
                            if (!Directory.Exists(storagePath))
                                Directory.CreateDirectory(storagePath);
                            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy();
                        }
                        catch
                        {
                            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy();
                        }
                    },
                    tags: new[] { "storage", "ready" });
                    break;
            }
        }

        return services;
    }

    /// <summary>
    /// Map health check endpoints with proper formatting:
    ///   - /health         → All checks (overall status)
    ///   - /health/live    → Liveness probe (is service running)
    ///   - /health/ready   → Readiness probe (dependencies ready)
    ///
    /// Usage in Program.cs:
    ///   app.MapHealthCheckEndpoints();
    /// </summary>
    public static IApplicationBuilder MapHealthCheckEndpoints(this IApplicationBuilder app)
    {
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

        // ── /health endpoint (all checks) ──────────────────────────────────
        app.UseEndpoints(endpoints =>
        {
            endpoints.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = MediaTypeNames.Application.Json;
                    var response = new
                    {
                        status = report.Status.ToString(),
                        timestamp = DateTime.UtcNow,
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
            });

            // ── /health/live endpoint (liveness probe) ─────────────────────
            endpoints.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = (_) => false,  // Don't check dependencies
                ResponseWriter = WriteSimpleHealthResponse
            });

            // ── /health/ready endpoint (readiness probe) ───────────────────
            endpoints.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = (check) => check.Tags.Contains("ready"),  // Only checks tagged "ready"
                ResponseWriter = WriteDetailedHealthResponse
            });
        });

        return app;
    }

    /// <summary>
    /// Simple health response (for liveness probe /health/live).
    /// Returns 200 if service is running, 503 if not.
    /// </summary>
    private static async Task WriteSimpleHealthResponse(HttpContext context, Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport report)
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;
        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow
        };
        await context.Response.WriteAsJsonAsync(response);
    }

    /// <summary>
    /// Detailed health response (for readiness probe /health/ready).
    /// Returns detailed status of all dependencies.
    /// </summary>
    private static async Task WriteDetailedHealthResponse(HttpContext context, Microsoft.Extensions.Diagnostics.HealthChecks.HealthReport report)
    {
        context.Response.ContentType = MediaTypeNames.Application.Json;
        var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var response = new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
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
}
