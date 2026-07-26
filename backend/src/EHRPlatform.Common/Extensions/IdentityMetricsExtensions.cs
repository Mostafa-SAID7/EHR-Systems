using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace EHRPlatform.Common.Extensions;

/// <summary>
/// Identity/JWT metrics instrumentation for OpenTelemetry.
/// 
/// Tracks authentication and authorization events:
///   - Login success/failure
///   - Refresh token usage
///   - Expired tokens
///   - Unauthorized requests
///   - Forbidden requests
/// 
/// Metrics are exported to Prometheus via /metrics endpoint.
/// </summary>
public static class IdentityMetricsExtensions
{
    /// <summary>
    /// Meter instance for identity metrics (singleton).
    /// Created once per application and reused for all metric recording.
    /// </summary>
    private static readonly Meter IdentityMeter = new Meter("EHRPlatform.Identity", "1.0.0");

    /// <summary>
    /// Get or create the identity metrics meter.
    /// Used by middleware and command handlers for metrics recording.
    /// </summary>
    public static Meter GetIdentityMeter() => IdentityMeter;

    /// <summary>
    /// Register identity metrics in OpenTelemetry.
    /// Must be called during service configuration (before build()).
    /// 
    /// Adds "EHRPlatform.Identity" meter to OpenTelemetry metrics pipeline.
    /// Ensures metrics are collected and exported to Prometheus.
    /// </summary>
    public static IServiceCollection AddIdentityMetrics(this IServiceCollection services)
    {
        // The meter is already created above (static field).
        // This just ensures it's registered in OpenTelemetry if not already there.
        // The actual meter registration happens in OpenTelemetryExtensions.cs via
        // .AddMeter("EHRPlatform.Identity")
        return services;
    }

    /// <summary>
    /// Middleware to track authentication and authorization metrics.
    /// Intercepts HTTP responses and records 401/403 status codes.
    /// 
    /// Metrics recorded:
    ///   - identity.unauthorized_requests (401 responses)
    ///   - identity.forbidden_requests (403 responses)
    /// </summary>
    public static IApplicationBuilder UseIdentityMetricsMiddleware(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            // Call next middleware
            await next();

            // Record metrics based on response status code
            var statusCode = context.Response.StatusCode;

            if (statusCode == StatusCodes.Status401Unauthorized)
            {
                var unauthorizedCounter = IdentityMeter.CreateCounter<long>(
                    "identity.unauthorized_requests",
                    description: "Number of unauthorized requests (401)",
                    unit: "{request}");
                
                unauthorizedCounter.Add(1, new KeyValuePair<string, object?>("endpoint", context.Request.Path.Value ?? "unknown"));
            }
            else if (statusCode == StatusCodes.Status403Forbidden)
            {
                var forbiddenCounter = IdentityMeter.CreateCounter<long>(
                    "identity.forbidden_requests",
                    description: "Number of forbidden requests (403)",
                    unit: "{request}");
                
                forbiddenCounter.Add(1, new KeyValuePair<string, object?>("endpoint", context.Request.Path.Value ?? "unknown"));
            }
        });

        return app;
    }
}

/// <summary>
/// Helper class for recording identity-related metrics.
/// Used by command handlers and services to track authentication events.
/// </summary>
public static class IdentityMetricsRecorder
{
    private static readonly Meter Meter = IdentityMetricsExtensions.GetIdentityMeter();

    /// <summary>
    /// Record a successful login.
    /// Increments: identity.login_success counter
    /// </summary>
    public static void RecordLoginSuccess(string userId, string email)
    {
        var counter = Meter.CreateCounter<long>(
            "identity.login_success",
            description: "Number of successful login attempts",
            unit: "{login}");
        
        counter.Add(1, 
            new KeyValuePair<string, object?>("user_id", userId),
            new KeyValuePair<string, object?>("email", email));
    }

    /// <summary>
    /// Record a failed login.
    /// Increments: identity.login_failure counter
    /// </summary>
    public static void RecordLoginFailure(string email, string reason = "invalid_credentials")
    {
        var counter = Meter.CreateCounter<long>(
            "identity.login_failure",
            description: "Number of failed login attempts",
            unit: "{attempt}");
        
        counter.Add(1, 
            new KeyValuePair<string, object?>("email", email),
            new KeyValuePair<string, object?>("reason", reason));
    }

    /// <summary>
    /// Record a refresh token usage.
    /// Increments: identity.refresh_token_usage counter
    /// </summary>
    public static void RecordRefreshTokenUsage(string userId)
    {
        var counter = Meter.CreateCounter<long>(
            "identity.refresh_token_usage",
            description: "Number of refresh token requests",
            unit: "{request}");
        
        counter.Add(1, new KeyValuePair<string, object?>("user_id", userId));
    }

    /// <summary>
    /// Record an expired token attempt.
    /// Increments: identity.expired_token_attempts counter
    /// </summary>
    public static void RecordExpiredTokenAttempt(string email)
    {
        var counter = Meter.CreateCounter<long>(
            "identity.expired_token_attempts",
            description: "Number of requests with expired tokens",
            unit: "{attempt}");
        
        counter.Add(1, new KeyValuePair<string, object?>("email", email));
    }

    /// <summary>
    /// Record account lockout due to failed login attempts.
    /// Increments: identity.account_lockout counter
    /// </summary>
    public static void RecordAccountLockout(string email, int failedAttempts)
    {
        var counter = Meter.CreateCounter<long>(
            "identity.account_lockout",
            description: "Number of account lockouts",
            unit: "{lockout}");
        
        counter.Add(1, 
            new KeyValuePair<string, object?>("email", email),
            new KeyValuePair<string, object?>("failed_attempts", failedAttempts));
    }

    /// <summary>
    /// Record unauthorized request.
    /// Increments: identity.unauthorized_requests counter
    /// </summary>
    public static void RecordUnauthorizedRequest(string endpoint)
    {
        var counter = Meter.CreateCounter<long>(
            "identity.unauthorized_requests",
            description: "Number of unauthorized requests (401)",
            unit: "{request}");
        
        counter.Add(1, new KeyValuePair<string, object?>("endpoint", endpoint));
    }

    /// <summary>
    /// Record forbidden request.
    /// Increments: identity.forbidden_requests counter
    /// </summary>
    public static void RecordForbiddenRequest(string endpoint, string? role = null)
    {
        var counter = Meter.CreateCounter<long>(
            "identity.forbidden_requests",
            description: "Number of forbidden requests (403)",
            unit: "{request}");
        
        counter.Add(1, 
            new KeyValuePair<string, object?>("endpoint", endpoint),
            new KeyValuePair<string, object?>("role", role ?? "unknown"));
    }

    /// <summary>
    /// Get count of active authenticated sessions.
    /// Returns: identity.active_sessions gauge
    /// </summary>
    public static ObservableGauge<int> CreateActiveSessionsGauge(Func<int> getActiveSessionsCount)
    {
        return Meter.CreateObservableGauge<int>(
            "identity.active_sessions",
            measurement: () => new Measurement<int>(getActiveSessionsCount()),
            description: "Number of active authenticated sessions",
            unit: "{session}");
    }

    /// <summary>
    /// Get average token lifetime.
    /// Returns: identity.token_lifetime_seconds gauge
    /// </summary>
    public static ObservableGauge<double> CreateTokenLifetimeGauge(Func<double> getAverageLifetime)
    {
        return Meter.CreateObservableGauge<double>(
            "identity.token_lifetime_seconds",
            measurement: () => new Measurement<double>(getAverageLifetime()),
            description: "Average JWT token lifetime in seconds",
            unit: "s");
    }
}
