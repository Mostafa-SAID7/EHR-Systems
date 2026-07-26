using OpenTelemetry.Exporter.Prometheus;
using OpenTelemetry.Metrics;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.Metrics;

namespace EHRPlatform.Common.Extensions;

/// <summary>
/// OpenTelemetry extensions for metrics instrumentation.
/// Provides standardized metric setup across all microservices.
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Add OpenTelemetry metrics with Prometheus exporter.
    /// Exposes /metrics endpoint for Prometheus scraping.
    /// 
    /// Metrics collected:
    /// - HTTP: request count, duration, status codes
    /// - Runtime: GC collections, memory, threads
    /// - Process: CPU time, private memory
    /// - ASP.NET Core: requests, exceptions, connection count
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="serviceName">Service/application name (appears in metrics labels)</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddOpenTelemetryMetrics(
        this IServiceCollection services,
        string serviceName)
    {
        services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics
                    // ASP.NET Core: HTTP request metrics
                    // - http.request.duration (histogram)
                    // - http.request.body.size (histogram)
                    // - http.response.body.size (histogram)
                    // - http.server.request.duration (histogram) — OpenTelemetry standard
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        // Filter health check endpoints from metrics (reduce noise)
                        options.Filter = context =>
                            !context.Request.Path.StartsWithSegments("/health") &&
                            !context.Request.Path.StartsWithSegments("/metrics");
                    })

                    // HTTP Client: outbound HTTP call metrics
                    // - http.client.request.duration (histogram)
                    // - http.client.request.body.size (histogram)
                    // - http.client.response.body.size (histogram)
                    .AddHttpClientInstrumentation(options =>
                    {
                        // Filter internal/self calls if needed
                        options.Filter = _ => true;
                    })

                    // MassTransit/RabbitMQ: message broker metrics
                    // - messaging.publish.messages (counter - publish rate)
                    // - messaging.receive.messages (counter - receive rate)
                    // - messaging.acknowledge (counter - ack rate)
                    // - rabbitmq.queue.message_count (gauge - queue length)
                    // - rabbitmq.consumer_count (gauge - consumer count)
                    // - rabbitmq.message.dead_letter (counter - dead-letter messages)
                    // - rabbitmq.message.redelivered (counter - redelivered messages)
                    .AddMeter("MassTransit")  // MassTransit activity diagnostics
                    .AddMeter("MassTransit.RabbitMQ")  // RabbitMQ-specific metrics
                    .AddMeter("System.Net.NameResolution")  // DNS metrics

                    // Identity/JWT: authentication and authorization metrics
                    // - identity.login_success (counter)
                    // - identity.login_failure (counter)
                    // - identity.refresh_token_usage (counter)
                    // - identity.expired_token_attempts (counter)
                    // - identity.account_lockout (counter)
                    // - identity.unauthorized_requests (counter)
                    // - identity.forbidden_requests (counter)
                    // - identity.active_sessions (gauge)
                    // - identity.token_lifetime_seconds (gauge)
                    .AddMeter("EHRPlatform.Identity")  // JWT/Identity metrics

                    // Runtime: CLR (Common Language Runtime) metrics
                    // - dotnet.gc.collections.count (counter)
                    // - dotnet.gc.objects.collected (histogram)
                    // - dotnet.gc.heap.total_allocated_bytes (counter)
                    // - dotnet.gc.last_collection.pause_duration (histogram)
                    // - dotnet.mem.committed (gauge)
                    .AddRuntimeInstrumentation()

                    // Process: system process metrics
                    // - process.cpu.time (counter)
                    // - process.cpu.utilization (gauge) — % of cores
                    // - process.memory.physical_usage_bytes (gauge)
                    // - process.memory.virtual_usage_bytes (gauge)
                    // - process.disk.operations (counter)
                    // - process.disk.io_bytes (counter)
                    .AddProcessInstrumentation()

                    // Prometheus: export metrics in Prometheus format
                    // Exposes /metrics endpoint with all collected metrics
                    .AddPrometheusExporter(options =>
                    {
                        options.ScrapeResponseCacheDurationMilliseconds = 0; // No caching (fresh on each scrape)
                    });

                // Add resource attributes (labels all metrics with service name and version)
                metrics.AddResource(r =>
                    r.AddService(serviceName, version: "1.0.0")
                     .AddAttributes(new[] {
                         KeyValuePair.Create("deployment.environment", GetEnvironment()),
                         KeyValuePair.Create("service.namespace", "ehr-platform")
                     }));
            });

        return services;
    }

    /// <summary>
    /// Map Prometheus metrics endpoint.
    /// Must be called on WebApplication BEFORE app.Run().
    /// </summary>
    /// <param name="app">WebApplication</param>
    /// <returns>WebApplication for chaining</returns>
    public static WebApplication MapPrometheusMetricsEndpoint(this WebApplication app)
    {
        // Map Prometheus scraping endpoint
        // Prometheus will scrape: GET /metrics
        app.MapPrometheusScrapingEndpoint();
        return app;
    }

    /// <summary>
    /// Get current environment (Development, Staging, Production).
    /// Defaults to Development if not set.
    /// </summary>
    private static string GetEnvironment()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("ENVIRONMENT")
            ?? "Development";
    }
}
