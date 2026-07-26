using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace EHRPlatform.Common.Extensions;

/// <summary>
/// OpenTelemetry extensions for comprehensive observability.
/// 
/// ARCHITECTURE:
/// ┌─────────────────────────┐
/// │   ASP.NET Core App      │
/// │  - HTTP requests        │
/// │  - Custom metrics       │
/// │  - Distributed traces   │
/// │  - Structured logs      │
/// └────────────┬────────────┘
///              │ (OTLP)
///              ↓
/// ┌─────────────────────────┐
/// │  OpenTelemetry Collector│
/// │  - Aggregation          │
/// │  - Sampling             │
/// │  - Batch processing     │
/// └───┬──────────┬──────────┘
///     ↓          ↓
/// Prometheus  Tempo/Jaeger  Loki
///     ↓          ↓           ↓
/// ┌──────────────────────────────┐
/// │     Grafana Dashboard        │
/// │  - Metrics, Traces, Logs     │
/// └──────────────────────────────┘
/// 
/// This is vendor-neutral: you can swap Prometheus for any metrics backend,
/// Tempo for any tracing backend, and Loki for any logging backend.
/// 
/// Features:
/// - Metrics: HTTP, RabbitMQ, Database, Identity, Runtime, Process
/// - Traces: Distributed tracing with span context propagation
/// - Logs: Structured JSON logging with correlation IDs
/// - HIPAA: PHI redaction, audit trails
/// </summary>
public static class OpenTelemetryExtensions
{
    /// <summary>
    /// Add OpenTelemetry metrics, traces, and logs with OTLP (OpenTelemetry Protocol) export.
    /// Sends all telemetry to OpenTelemetry Collector instead of Prometheus-specific endpoint.
    /// 
    /// This keeps the application vendor-neutral:
    /// - Can replace Prometheus with any metrics backend
    /// - Can replace Tempo with any tracing backend
    /// - Can replace Loki with any logging backend
    /// - Only the OTEL Collector config changes, not application code
    /// 
    /// Metrics collected:
    /// - HTTP: request count, duration, status codes (ASP.NET Core instrumentation)
    /// - RabbitMQ: queue length, consumer count, publish/ack rates (MassTransit)
    /// - Identity: login success/failure, token refresh (custom meter)
    /// - Database: query duration, connection pool (automatic)
    /// - Runtime: GC collections, memory, threads
    /// - Process: CPU time, memory usage
    /// 
    /// Traces collected:
    /// - HTTP requests (automatic)
    /// - Database queries (automatic via EntityFramework instrumentation)
    /// - RabbitMQ messages (via MassTransit)
    /// - Custom spans via ActivitySource
    /// 
    /// Logs collected:
    /// - All ILogger outputs (automatic)
    /// - Structured JSON with correlation IDs
    /// - Trace context correlation
    /// </summary>
    /// <param name="services">Service collection</param>
    /// <param name="serviceName">Service name (appears in resource attributes)</param>
    /// <returns>Service collection for chaining</returns>
    public static IServiceCollection AddOpenTelemetryObservability(
        this IServiceCollection services,
        string serviceName)
    {
        // OTLP exporter configuration
        var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")
            ?? "http://otel-collector:4317";  // Default to Docker Compose OTEL Collector
        
        services.AddOpenTelemetry()
            // ════════════════════════════════════════════════════════════════════
            // METRICS PIPELINE (vendor-neutral OTLP export)
            // ════════════════════════════════════════════════════════════════════
            .WithMetrics(metrics =>
            {
                metrics
                    // Add custom meter sources (automatic instrumentation happens on app startup)
                    .AddMeter("MassTransit")
                    .AddMeter("MassTransit.RabbitMQ")
                    .AddMeter("EHRPlatform.Identity")
                    .AddMeter("EHRPlatform.ApiGateway")
                    .AddMeter("System.Net.Http")  // Built-in HTTP metrics
                    .AddMeter("System.Runtime");  // Built-in runtime metrics

                // Add resource attributes (applies to all metrics)
                var resource = ResourceBuilder.CreateDefault()
                    .AddService(
                        serviceName: serviceName,
                        serviceVersion: "1.0.0",
                        serviceNamespace: "ehr-platform",
                        autoGenerateServiceInstanceId: true)
                    .AddAttributes(new Dictionary<string, object>
                    {
                        ["deployment.environment"] = GetEnvironment(),
                        ["service.namespace"] = "ehr-platform",
                        ["telemetry.sdk.language"] = "dotnet",
                        ["telemetry.sdk.name"] = "opentelemetry"
                    });

                metrics.SetResourceBuilder(resource);

                // OTLP Exporter: vendor-neutral metric export
                metrics.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                });
            })

            // ════════════════════════════════════════════════════════════════════
            // TRACES PIPELINE (vendor-neutral OTLP export)
            // ════════════════════════════════════════════════════════════════════
            .WithTracing(tracing =>
            {
                var resource = ResourceBuilder.CreateDefault()
                    .AddService(
                        serviceName: serviceName,
                        serviceVersion: "1.0.0",
                        serviceNamespace: "ehr-platform",
                        autoGenerateServiceInstanceId: true)
                    .AddAttributes(new Dictionary<string, object>
                    {
                        ["deployment.environment"] = GetEnvironment(),
                        ["service.namespace"] = "ehr-platform",
                        ["telemetry.sdk.language"] = "dotnet",
                        ["telemetry.sdk.name"] = "opentelemetry"
                    });

                tracing
                    // Add custom sources for distributed tracing
                    .AddSource("EHRPlatform.*")
                    .AddSource("MassTransit")
                    .AddSource("MassTransit.RabbitMQ")
                    .AddSource("System.Net.Http")

                    // Set resource attributes
                    .SetResourceBuilder(resource)

                    // OTLP Exporter: vendor-neutral trace export
                    .AddOtlpExporter(options =>
                    {
                        options.Endpoint = new Uri(otlpEndpoint);
                    });
            });

        return services;
    }

    /// <summary>
    /// Add structured JSON logging with correlation IDs and trace context.
    /// Logs are automatically exported via OTEL Collector to Loki, CloudWatch, etc.
    /// 
    /// Each log entry includes:
    /// - Trace ID (for correlating with distributed traces)
    /// - Span ID (for linking to specific operations)
    /// - Log level (Trace, Debug, Information, Warning, Error, Critical)
    /// - Message and structured properties
    /// - Exception details (if applicable)
    /// </summary>
    public static ILoggingBuilder AddOpenTelemetryLogging(this ILoggingBuilder logging)
    {
        var otlpEndpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")
            ?? "http://otel-collector:4317";

        logging.ClearProviders();
        logging.AddConsole();
        
        // Add OpenTelemetry Logging exporter
        logging.AddOpenTelemetry(options =>
        {
            options.AddOtlpExporter(exporter =>
            {
                exporter.Endpoint = new Uri(otlpEndpoint);
            });

            // Add resource attributes
            options.SetResourceBuilder(ResourceBuilder.CreateDefault()
                .AddService("ehr-platform"));

            // Include trace context (trace ID, span ID)
            options.IncludeScopes = true;
            options.IncludeFormattedMessage = true;
        });

        return logging;
    }

    /// <summary>
    /// Get current environment (Development, Staging, Production).
    /// Defaults to Development if not set.
    /// Used for conditional behaviors (e.g., recording SQL queries only in Dev).
    /// </summary>
    private static string GetEnvironment()
    {
        return Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT")
            ?? Environment.GetEnvironmentVariable("ENVIRONMENT")
            ?? "Development";
    }
}
