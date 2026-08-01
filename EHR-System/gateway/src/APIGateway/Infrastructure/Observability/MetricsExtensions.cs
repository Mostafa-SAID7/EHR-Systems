namespace EHRPlatform.Gateway.Infrastructure.Observability;

/// <summary>
/// Extension methods for metrics registration and middleware setup.
/// </summary>
public static class MetricsExtensions
{
    /// <summary>
    /// Register gateway metrics and OpenTelemetry instrumentation.
    /// </summary>
    public static IServiceCollection AddGatewayMetrics(this IServiceCollection services)
    {
        services.AddSingleton<IGatewayMetrics, GatewayMetrics>();
        
        // Register OpenTelemetry metrics
        services.AddOpenTelemetry()
            .WithMetrics(meter => meter
                .AddMeter("EHRPlatform.Gateway")
                .AddRuntimeInstrumentation()
                .AddHttpClientInstrumentation()
                .AddAspNetCoreInstrumentation()
                .AddPrometheusExporter());

        return services;
    }

    /// <summary>
    /// Register metrics middleware in the request pipeline.
    /// Exposes Prometheus metrics endpoint at /metrics
    /// </summary>
    public static IApplicationBuilder UseGatewayMetrics(this IApplicationBuilder app)
    {
        app.UseMiddleware<MetricsMiddleware>();
        return app;
    }
}
