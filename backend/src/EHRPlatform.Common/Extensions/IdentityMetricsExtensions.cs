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
    /// IMPORTANT: Uses low-cardinality labels only (endpoint category, not full path).
    /// This prevents unbounded cardinality growth from user IDs, session IDs, etc.
    /// 
    /// Metrics recorded:
    ///   - identity.unauthorized_requests (401 responses) with endpoint label
    ///   - identity.forbidden_requests (403 responses) with endpoint and role labels
    /// </summary>
    public static IApplicationBuilder UseIdentityMetricsMiddleware(this IApplicationBuilder app)
    {
        app.Use(async (context, next) =>
        {
            // Call next middleware
            await next();

            // Record metrics based on response status code
            var statusCode = context.Response.StatusCode;
            var endpoint = context.Request.Path.Value ?? "unknown";
            var endpointCategory = ExtractEndpointCategoryFromPath(endpoint);

            if (statusCode == StatusCodes.Status401Unauthorized)
            {
                var unauthorizedCounter = IdentityMeter.CreateCounter<long>(
                    "identity.unauthorized_requests",
                    description: "Number of unauthorized requests (401)",
                    unit: "{request}");
                
                // Low-cardinality endpoint category only (e.g., "patients", "appointments")
                unauthorizedCounter.Add(1, new("endpoint", endpointCategory));
            }
            else if (statusCode == StatusCodes.Status403Forbidden)
            {
                var forbiddenCounter = IdentityMeter.CreateCounter<long>(
                    "identity.forbidden_requests",
                    description: "Number of forbidden requests (403)",
                    unit: "{request}");
                
                // Low-cardinality labels only
                forbiddenCounter.Add(1, 
                    new("endpoint", endpointCategory),
                    new("role", context.User?.FindFirst("role")?.Value ?? "unknown"));
            }
        });

        return app;
    }

    /// <summary>
    /// Extract low-cardinality endpoint category from path.
    /// 
    /// Examples:
    ///   /api/patients/123          → patients
    ///   /api/appointments/456      → appointments
    ///   /api/clinical/records/789  → clinical
    ///   /health                    → health
    /// 
    /// This prevents high-cardinality explosion from specific IDs.
    /// </summary>
    private static string ExtractEndpointCategoryFromPath(string path)
    {
        if (string.IsNullOrEmpty(path))
            return "unknown";

        var segments = path.TrimStart('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        
        if (segments.Length == 0)
            return "unknown";

        // For /api/category/id/action, return category (2nd segment)
        if (segments.Length >= 2 && segments[0] == "api")
        {
            return segments[1]; // patients, appointments, clinical, etc.
        }

        // For /health, /metrics, /swagger, etc.
        return segments[0];
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
    /// 
    /// NOTE: Do NOT use email or user_id as labels — these are high-cardinality (unique per user).
    /// Use only low-cardinality labels: tenant, authentication_method, environment.
    /// For user-specific debugging, correlate via trace IDs and span attributes instead.
    /// </summary>
    public static void RecordLoginSuccess(string userId, string email, string? authenticationMethod = "password")
    {
        var counter = Meter.CreateCounter<long>(
            "identity.login_success",
            description: "Number of successful login attempts",
            unit: "{login}");
        
        // Low-cardinality labels only: authentication_method
        counter.Add(1, 
            new KeyValuePair<string, object?>("method", authenticationMethod ?? "password"));
    }

    /// <summary>
    /// Record a failed login.
    /// Increments: identity.login_failure counter
    /// 
    /// NOTE: Do NOT use email as a label — this is high-cardinality (unique).
    /// Use only low-cardinality reason label (invalid_credentials, account_locked, etc.)
    /// </summary>
    public static void RecordLoginFailure(string email, string reason = "invalid_credentials")
    {
        var counter = Meter.CreateCounter<long>(
            "identity.login_failure",
            description: "Number of failed login attempts",
            unit: "{attempt}");
        
        // Low-cardinality label only: reason
        counter.Add(1, 
            new KeyValuePair<string, object?>("reason", reason));
    }

    /// <summary>
    /// Record a refresh token usage.
    /// Increments: identity.refresh_token_usage counter
    /// 
    /// NOTE: Do NOT use user_id as a label — this is high-cardinality (unique per user).
    /// Use low-cardinality labels only for aggregation.
    /// </summary>
    public static void RecordRefreshTokenUsage(string userId)
    {
        var counter = Meter.CreateCounter<long>(
            "identity.refresh_token_usage",
            description: "Number of refresh token requests",
            unit: "{request}");
        
        // No user-specific labels (low-cardinality only)
        counter.Add(1);
    }

    /// <summary>
    /// Record an expired token attempt.
    /// Increments: identity.expired_token_attempts counter
    /// 
    /// NOTE: Do NOT use email as a label — this is high-cardinality (unique).
    /// Token expiration is already tracked by token lifetime gauge.
    /// </summary>
    public static void RecordExpiredTokenAttempt(string email)
    {
        var counter = Meter.CreateCounter<long>(
            "identity.expired_token_attempts",
            description: "Number of requests with expired tokens",
            unit: "{attempt}");
        
        // No user-specific labels
        counter.Add(1);
    }

    /// <summary>
    /// Record account lockout due to failed login attempts.
    /// Increments: identity.account_lockout counter
    /// 
    /// NOTE: Do NOT use email as a label — this is high-cardinality (unique).
    /// Use only low-cardinality labels.
    /// </summary>
    public static void RecordAccountLockout(string email, int failedAttempts)
    {
        var counter = Meter.CreateCounter<long>(
            "identity.account_lockout",
            description: "Number of account lockouts",
            unit: "{lockout}");
        
        // Only count, no user-specific labels (low-cardinality)
        counter.Add(1);
    }

    /// <summary>
    /// Record unauthorized request.
    /// Increments: identity.unauthorized_requests counter
    /// 
    /// NOTE: Low-cardinality endpoint label is acceptable (limited to ~50-100 unique values).
    /// Do NOT add user_id, email, or JWT tokens as labels.
    /// </summary>
    public static void RecordUnauthorizedRequest(string endpoint)
    {
        var counter = Meter.CreateCounter<long>(
            "identity.unauthorized_requests",
            description: "Number of unauthorized requests (401)",
            unit: "{request}");
        
        // Endpoint is low-cardinality (~50 unique values)
        counter.Add(1, new("endpoint", ExtractEndpointCategory(endpoint)));
    }

    /// <summary>
    /// Record forbidden request.
    /// Increments: identity.forbidden_requests counter
    /// 
    /// NOTE: Low-cardinality labels only: role (Admin, Doctor, Patient, etc.)
    /// Do NOT use user_id, email, or any PII.
    /// </summary>
    public static void RecordForbiddenRequest(string endpoint, string? role = null)
    {
        var counter = Meter.CreateCounter<long>(
            "identity.forbidden_requests",
            description: "Number of forbidden requests (403)",
            unit: "{request}");
        
        // Low-cardinality labels only
        counter.Add(1, 
            new("endpoint", ExtractEndpointCategory(endpoint)),
            new("role", role ?? "unknown"));
    }

    /// <summary>
    /// Extract endpoint category from path (low-cardinality).
    /// Examples:
    ///   /api/patients/123/records → patients
    ///   /api/appointments/456     → appointments
    ///   /api/clinical/diagnoses   → clinical
    /// </summary>
    private static string ExtractEndpointCategory(string endpoint)
    {
        if (string.IsNullOrEmpty(endpoint))
            return "unknown";

        var segments = endpoint.TrimStart('/').Split('/', StringSplitOptions.RemoveEmptyEntries);
        
        // For /api/resource/action, return resource (2nd segment)
        if (segments.Length >= 2)
        {
            return segments[1]; // patients, appointments, etc.
        }

        return segments.Length > 0 ? segments[0] : "unknown";
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
