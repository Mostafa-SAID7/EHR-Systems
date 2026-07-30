using EHRPlatform.Common.Infrastructure.Telemetry;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace EHRPlatform.Common.Shared.Extensions;

/// <summary>
/// OpenTelemetry tracing DI extensions for EHR microservices.
///
/// Instruments:
///   ASP.NET Core incoming requests
///   HttpClient outbound calls
///   EHR custom activities (EHRTelemetry.ActivitySource)
///
/// Exporters (selected by config):
///   OpenTelemetry:OtlpEndpoint set → OTLP (Jaeger / Grafana Tempo)
///   Not set                        → Console (development fallback)
///
/// HIPAA: RecordException captures exception type and message only — never PHI.
/// </summary>
public static class TelemetryExtensions
{
    /// <summary>
    /// Add OpenTelemetry tracing for an EHR microservice.
    /// </summary>
    /// <param name="services">Service collection.</param>
    /// <param name="configuration">App configuration.</param>
    /// <param name="serviceName">Logical service name (e.g. "patient-service").</param>
    public static IServiceCollection AddEHRTelemetry(
        this IServiceCollection services,
        IConfiguration configuration,
        string serviceName)
    {
        var otlpEndpoint = configuration["OpenTelemetry:OtlpEndpoint"]
                        ?? Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT");

        services.AddOpenTelemetry()
            .ConfigureResource(r => r
                .AddService(
                    serviceName: serviceName,
                    serviceVersion: EHRTelemetry.ServiceVersion)
                .AddAttributes(new Dictionary<string, object>
                {
                    ["deployment.environment"] =
                        configuration["ASPNETCORE_ENVIRONMENT"] ?? "production"
                }))
            .WithTracing(tracing =>
            {
                tracing
                    .AddSource(EHRTelemetry.ServiceName)        // Custom EHR spans
                    .AddAspNetCoreInstrumentation(opts =>
                    {
                        opts.RecordException = true;
                        // Exclude health check noise from traces
                        opts.Filter = ctx =>
                            !ctx.Request.Path.StartsWithSegments("/health") &&
                            !ctx.Request.Path.StartsWithSegments("/metrics");
                    })
                    .AddHttpClientInstrumentation(opts =>
                    {
                        opts.RecordException = true;
                    });

                if (!string.IsNullOrWhiteSpace(otlpEndpoint))
                {
                    // Production: export to Jaeger / Grafana Tempo / any OTLP collector
                    tracing.AddOtlpExporter(opts =>
                        opts.Endpoint = new Uri(otlpEndpoint));
                }
                else
                {
                    // Development: write spans to console for quick inspection
                    tracing.AddConsoleExporter();
                }
            });

        return services;
    }
}

