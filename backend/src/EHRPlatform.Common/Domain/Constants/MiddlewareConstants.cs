#nullable enable

namespace EHRPlatform.Common.Domain.Constants;

/// <summary>
/// HTTP header and middleware constants.
/// Defines standard header names and context keys.
/// Single responsibility: Define middleware constants only.
/// </summary>
public static class MiddlewareConstants
{
    /// <summary>HTTP header name for correlation ID.</summary>
    public const string CorrelationIdHeaderName = "X-Correlation-ID";

    /// <summary>HTTP header name for request ID.</summary>
    public const string RequestIdHeaderName = "X-Request-ID";

    /// <summary>LogContext key for correlation ID.</summary>
    public const string CorrelationIdContextKey = "CorrelationId";

    /// <summary>HTTP header name for tenant ID.</summary>
    public const string TenantIdHeaderName = "X-Tenant-ID";

    /// <summary>HTTP header name for user ID.</summary>
    public const string UserIdHeaderName = "X-User-ID";
}
