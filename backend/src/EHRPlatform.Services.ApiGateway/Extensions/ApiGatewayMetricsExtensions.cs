using System.Diagnostics;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.Metrics;

namespace EHRPlatform.Services.ApiGateway.Extensions;

/// <summary>
/// API Gateway metrics collection via custom middleware.
/// Tracks gateway-specific metrics that reverse proxy (YARP) doesn't expose.
/// 
/// Metrics collected:
///   - gateway_requests_per_second (counter - requests/sec)
///   - gateway_latency_seconds (histogram - gateway latency)
///   - route_latency_seconds (histogram - per-route latency)
///   - gateway_auth_failures_total (counter - authentication failures)
///   - gateway_authz_failures_total (counter - authorization failures)
///   - gateway_http_5xx_total (counter - 5xx errors)
///   - gateway_http_4xx_total (counter - 4xx errors)
/// </summary>
public static class ApiGatewayMetricsExtensions
{
    private static readonly ActivitySource GatewayActivitySource = 
        new ActivitySource("EHRPlatform.ApiGateway.Metrics");
    
    private static Meter? _gatewayMeter;
    private static Counter<long>? _requestsCounter;
    private static Histogram<double>? _gatewayLatencyHistogram;
    private static Histogram<double>? _routeLatencyHistogram;
    private static Counter<long>? _authFailuresCounter;
    private static Counter<long>? _authzFailuresCounter;
    private static Counter<long>? _errors5xxCounter;
    private static Counter<long>? _errors4xxCounter;

    /// <summary>
    /// Register API Gateway metrics collection.
    /// Call this after AddOpenTelemetryMetrics() to add custom gateway metrics to the same meter.
    /// </summary>
    public static IServiceCollection AddApiGatewayMetrics(this IServiceCollection services)
    {
        // Initialize meter and counters
        _gatewayMeter = new Meter("EHRPlatform.ApiGateway", "1.0.0");
        
        _requestsCounter = _gatewayMeter.CreateCounter<long>(
            "gateway_requests_total",
            unit: "requests",
            description: "Total gateway requests processed");

        _gatewayLatencyHistogram = _gatewayMeter.CreateHistogram<double>(
            "gateway_latency_seconds",
            unit: "s",
            description: "Gateway request latency (full round-trip)");

        _routeLatencyHistogram = _gatewayMeter.CreateHistogram<double>(
            "route_latency_seconds",
            unit: "s",
            description: "Per-route request latency");

        _authFailuresCounter = _gatewayMeter.CreateCounter<long>(
            "gateway_auth_failures_total",
            unit: "failures",
            description: "Total authentication failures (401 Unauthorized)");

        _authzFailuresCounter = _gatewayMeter.CreateCounter<long>(
            "gateway_authz_failures_total",
            unit: "failures",
            description: "Total authorization failures (403 Forbidden)");

        _errors5xxCounter = _gatewayMeter.CreateCounter<long>(
            "gateway_http_5xx_total",
            unit: "errors",
            description: "Total 5xx server errors");

        _errors4xxCounter = _gatewayMeter.CreateCounter<long>(
            "gateway_http_4xx_total",
            unit: "errors",
            description: "Total 4xx client errors (excluding 401/403)");

        // Store meters in service for middleware access
        services.AddSingleton(_gatewayMeter);
        services.AddSingleton(_requestsCounter);
        services.AddSingleton(_gatewayLatencyHistogram);
        services.AddSingleton(_routeLatencyHistogram);
        services.AddSingleton(_authFailuresCounter);
        services.AddSingleton(_authzFailuresCounter);
        services.AddSingleton(_errors5xxCounter);
        services.AddSingleton(_errors4xxCounter);

        return services;
    }

    /// <summary>
    /// Add API Gateway metrics collection middleware.
    /// Collects:
    ///   - Requests/sec (via counter)
    ///   - Gateway latency (via histogram)
    ///   - Route latency (via histogram with route label)
    ///   - Auth/Authz failures
    ///   - 4xx/5xx errors
    ///
    /// Call this EARLY in the middleware pipeline (after exception handler, before rate limiting).
    /// </summary>
    public static WebApplication UseApiGatewayMetrics(this WebApplication app)
    {
        app.Use(async (context, next) =>
        {
            var stopwatch = Stopwatch.StartNew();
            var routeName = context.Request.Path.ToString();
            
            try
            {
                await next(context);
            }
            finally
            {
                stopwatch.Stop();
                var latencySeconds = stopwatch.Elapsed.TotalSeconds;

                // Extract route name from path (e.g., /api/v1/patients → patients)
                var routeLabel = ExtractRouteLabel(routeName);

                // Record all metrics
                _requestsCounter?.Add(1, new[] { new KeyValuePair<string, object?>("route", routeLabel), new("service", "api-gateway") });
                _gatewayLatencyHistogram?.Record(latencySeconds, new KeyValuePair<string, object?>("route", routeLabel));
                _routeLatencyHistogram?.Record(latencySeconds, new KeyValuePair<string, object?>("route", routeLabel));

                // Record error metrics based on status code
                var statusCode = context.Response.StatusCode;
                if (statusCode == 401)
                {
                    _authFailuresCounter?.Add(1, new KeyValuePair<string, object?>("route", routeLabel));
                }
                else if (statusCode == 403)
                {
                    _authzFailuresCounter?.Add(1, new KeyValuePair<string, object?>("route", routeLabel));
                }
                else if (statusCode >= 500)
                {
                    // Use low-cardinality status label: 5xx (not specific status code)
                    // This prevents unbounded cardinality from future status codes
                    _errors5xxCounter?.Add(1, new KeyValuePair<string, object?>("route", routeLabel), new("status_class", "5xx"));
                }
                else if (statusCode >= 400 && statusCode != 401 && statusCode != 403)
                {
                    // Use low-cardinality status label: 4xx (not specific status code)
                    _errors4xxCounter?.Add(1, new KeyValuePair<string, object?>("route", routeLabel), new("status_class", "4xx"));
                }
            }
        });

        return app;
    }

    /// <summary>
    /// Extract route label from request path.
    /// Examples:
    ///   /api/v1/patients/123        → patients
    ///   /api/v1/clinical/records    → clinical
    ///   /swagger/index.html         → swagger
    ///   /health                     → health
    /// </summary>
    private static string ExtractRouteLabel(string path)
    {
        if (string.IsNullOrEmpty(path) || path == "/")
            return "root";

        // Remove leading slash
        var segments = path.TrimStart('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        
        if (segments.Length == 0)
            return "root";

        // For /api/v1/resource/action, return resource (3rd segment)
        // For /health, return health (1st segment)
        if (segments.Length >= 3 && segments[0] == "api")
        {
            // /api/v1/patients/123 → patients
            return segments[2];
        }

        // First non-API segment
        return segments[0];
    }
}
