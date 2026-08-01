namespace EHRPlatform.Gateway.Infrastructure.Observability;

/// <summary>
/// Metrics collector for API Gateway using OpenTelemetry.
/// Tracks request latency, throughput, errors, and service health.
/// Exports to Prometheus for monitoring and alerting.
/// </summary>
public interface IGatewayMetrics
{
    void RecordRequestDuration(string serviceName, string endpoint, long durationMs, int statusCode);
    void RecordRequestCount(string serviceName, string endpoint, int statusCode);
    void RecordActiveRequests(string serviceName, int delta);
    void RecordRateLimitExceeded(string userId);
    void RecordAuthenticationFailure(string reason);
    void RecordCacheHit(string cacheKey);
    void RecordCacheMiss(string cacheKey);
    void RecordCircuitBreakerStateChange(string serviceName, string state);
}
