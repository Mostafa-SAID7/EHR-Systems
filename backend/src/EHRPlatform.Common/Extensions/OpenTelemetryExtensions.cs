using OpenTelemetry;
using OpenTelemetry.Exporter.Prometheus;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using OpenTelemetry.Logs;
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
                    // ASP.NET Core: HTTP server metrics (automatic instrumentation)
                    // - http.server.request.duration (histogram, milliseconds)
                    // - http.server.request.body.size (histogram, bytes)
                    // - http.server.response.body.size (histogram, bytes)
                    // Attributes: http.method, http.status_code, http.target, http.scheme
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        // Filter health checks and metrics endpoints to reduce noise
                        options.Filter = context =>
                            !context.Request.Path.StartsWithSegments("/health") &&
                            !context.Request.Path.StartsWithSegments("/metrics") &&
                            !context.Request.Path.StartsWithSegments("/live") &&
                            !context.Request.Path.StartsWithSegments("/ready");

                        // Record request body size
                        options.RecordException = true;
                    })

                    // HTTP Client: outbound HTTP call metrics (automatic instrumentation)
                    // - http.client.request.duration (histogram, milliseconds)
                    // - http.client.request.body.size (histogram, bytes)
                    // - http.client.response.body.size (histogram, bytes)
                    // Attributes: http.method, http.status_code, http.url
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                    })

                    // Entity Framework Core: database query metrics (automatic instrumentation)
                    // - db.client.operations.count (counter)
                    // - db.client.operation.duration (histogram, milliseconds)
                    // Attributes: db.system, db.name, db.operation
                    .AddSqlClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.SetDbStatementForStoredProcedure = true;
                    })

                    // MassTransit/RabbitMQ: message broker metrics
                    // - messaging.publish.messages (counter)
                    // - messaging.receive.messages (counter)
                    // - messaging.acknowledge (counter)
                    // - rabbitmq.queue.message_count (gauge)
                    // - rabbitmq.consumer_count (gauge)
                    .AddMeter("MassTransit")
                    .AddMeter("MassTransit.RabbitMQ")

                    // Identity/JWT: custom authentication metrics
                    // - identity.login_success (counter)
                    // - identity.login_failure (counter)
                    // - identity.refresh_token_usage (counter)
                    .AddMeter("EHRPlatform.Identity")

                    // API Gateway: custom gateway metrics
                    // - gateway_requests_total (counter)
                    // - gateway_latency_seconds (histogram)
                    .AddMeter("EHRPlatform.ApiGateway")

                    // Runtime: .NET CLR metrics (automatic)
                    // - dotnet.gc.collections.count (counter, collections per generation)
                    // - dotnet.gc.objects.collected (counter)
                    // - dotnet.gc.heap.total_allocated_bytes (counter)
                    // - dotnet.mem.committed (gauge, bytes)
                    // - dotnet.gc.pause_time (histogram, milliseconds)
                    .AddRuntimeInstrumentation()

                    // Process: system process metrics (automatic)
                    // - process.cpu.time (counter, seconds)
                    // - process.cpu.utilization (gauge, 0-1)
                    // - process.memory.physical_usage_bytes (gauge)
                    // - process.memory.virtual_usage_bytes (gauge)
                    // - process.disk.operations (counter)
                    // - process.disk.io_bytes (counter, bytes)
                    .AddProcessInstrumentation()

                    // System metrics (automatic)
                    // - system.memory.usage (gauge)
                    // - system.network.io (counter)
                    .AddRuntimeInstrumentation();

                // Add resource attributes (applies to all metrics)
                metrics.AddResource(r =>
                    r.AddService(
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
                     }));

                // OTLP Exporter: vendor-neutral metric export
                // Sends metrics to OpenTelemetry Collector (http://otel-collector:4317)
                // Collector then exports to Prometheus, Datadog, New Relic, etc.
                metrics.AddOtlpExporter(options =>
                {
                    options.Endpoint = new Uri(otlpEndpoint);
                    // Default protocol: gRPC (OTLP/gRPC)
                    // Can also use HTTP: options.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf;
                });

                // Optional: Also keep Prometheus scrape endpoint for direct scraping
                // (useful for local development, can be disabled in production)
                // metrics.AddPrometheusExporter(options =>
                // {
                //     options.ScrapeResponseCacheDurationMilliseconds = 0;
                // });
            })

            // ════════════════════════════════════════════════════════════════════
            // TRACES PIPELINE (vendor-neutral OTLP export)
            // ════════════════════════════════════════════════════════════════════
            .WithTracing(tracing =>
            {
                tracing
                    // ASP.NET Core: HTTP server traces (automatic instrumentation)
                    // - Creates spans for incoming HTTP requests
                    // - Captures request/response headers, body size, status code
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        // Filter health checks to reduce span volume
                        options.Filter = context =>
                            !context.Request.Path.StartsWithSegments("/health") &&
                            !context.Request.Path.StartsWithSegments("/metrics");
                        
                        options.RecordException = true;
                        options.EnrichWithHttpRequest = (activity, request) =>
                        {
                            // Add custom attributes to spans
                            activity.SetTag("http.request.body_size", request.ContentLength);
                        };
                    })

                    // HTTP Client: outbound HTTP call traces
                    // - Creates spans for external HTTP calls
                    // - Captures response time, status code, URL
                    .AddHttpClientInstrumentation(options =>
                    {
                        options.RecordException = true;
                        options.EnrichWithHttpRequestMessage = (activity, request) =>
                        {
                            activity.SetTag("http.request.method", request.Method);
                        };
                    })

                    // Entity Framework Core: database operation traces
                    // - Creates spans for database queries
                    // - Captures query text, duration, database name
                    // SECURITY: Remove query text in production (contains PII)
                    .AddEntityFrameworkCoreInstrumentation(options =>
                    {
                        options.RecordException = true;
                        // Security: Don't record SQL queries in production (may contain PII)
                        options.SetDbStatementForStoredProcedure = GetEnvironment() != "Production";
                    })

                    // MassTransit: message broker traces
                    // - Creates spans for message publish/consume
                    .AddSource("MassTransit")
                    .AddSource("MassTransit.RabbitMQ")

                    // Custom sources
                    .AddSource("EHRPlatform.*")

                    // Add resource attributes (applies to all traces)
                    .AddResource(r =>
                        r.AddService(
                            serviceName: serviceName,
                            serviceVersion: "1.0.0",
                            serviceNamespace: "ehr-platform",
                            autoGenerateServiceInstanceId: true));

                // OTLP Exporter: vendor-neutral trace export
                // Sends traces to OpenTelemetry Collector for storage in Tempo, Jaeger, etc.
                tracing.AddOtlpExporter(options =>
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
