using System.Diagnostics.Metrics;

namespace EHRPlatform.Gateway.Infrastructure.Observability;

/// <summary>
/// Production implementation using OpenTelemetry Metrics.
/// </summary>
public class GatewayMetrics : IGatewayMetrics
{
    private readonly Meter _meter;
    private readonly ILogger<GatewayMetrics> _logger;

    // Histograms (for latency measurements)
    private readonly Histogram<long> _requestDurationMs;

    // Counters (for event counts)
    private readonly Counter<long> _requestsTotal;
    private readonly Counter<long> _rateLimitExceeded;
    private readonly Counter<long> _authenticationFailures;
    private readonly Counter<long> _cacheHits;
    private readonly Counter<long> _cacheMisses;

    // Gauge (for current values)
    private readonly UpDownCounter<long> _activeRequests;
    private readonly UpDownCounter<long> _circuitBreakerOpen;

    public GatewayMetrics(ILogger<GatewayMetrics> logger)
    {
        _logger = logger;

        // Create meter with OpenTelemetry conventions
        _meter = new Meter("EHRPlatform.Gateway", "1.0.0");

        // Initialize metrics
        _requestDurationMs = _meter.CreateHistogram<long>(
            "gateway.request.duration.ms",
            unit: "ms",
            description: "Request duration in milliseconds");

        _requestsTotal = _meter.CreateCounter<long>(
            "gateway.requests.total",
            description: "Total HTTP requests received by gateway");

        _activeRequests = _meter.CreateUpDownCounter<long>(
            "gateway.requests.active",
            description: "Currently active requests being processed");

        _rateLimitExceeded = _meter.CreateCounter<long>(
            "gateway.rate_limit.exceeded",
            description: "Number of requests rejected due to rate limiting");

        _authenticationFailures = _meter.CreateCounter<long>(
            "gateway.auth.failures",
            description: "Number of authentication failures");

        _cacheHits = _meter.CreateCounter<long>(
            "gateway.cache.hits",
            description: "Cache hits for response aggregation");

        _cacheMisses = _meter.CreateCounter<long>(
            "gateway.cache.misses",
            description: "Cache misses for response aggregation");

        _circuitBreakerOpen = _meter.CreateUpDownCounter<long>(
            "gateway.circuit_breaker.open",
            description: "Number of open circuit breakers");
    }

    public void RecordRequestDuration(string serviceName, string endpoint, long durationMs, int statusCode)
    {
        try
        {
            var tags = new TagList
            {
                { "service", serviceName },
                { "endpoint", endpoint },
                { "status_code", statusCode },
                { "status_class", GetStatusClass(statusCode) }
            };

            _requestDurationMs.Record(durationMs, tags);

            if (durationMs > 1000)
            {
                _logger.LogWarning(
                    "Slow request detected - Service: {ServiceName}, Endpoint: {Endpoint}, Duration: {DurationMs}ms, Status: {StatusCode}",
                    serviceName, endpoint, durationMs, statusCode);
            }

            if (statusCode >= 500)
            {
                _logger.LogError(
                    "Server error detected - Service: {ServiceName}, Endpoint: {Endpoint}, Status: {StatusCode}",
                    serviceName, endpoint, statusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording request duration metric");
        }
    }

    public void RecordRequestCount(string serviceName, string endpoint, int statusCode)
    {
        try
        {
            var tags = new TagList
            {
                { "service", serviceName },
                { "endpoint", endpoint },
                { "status_code", statusCode },
                { "status_class", GetStatusClass(statusCode) }
            };

            _requestsTotal.Add(1, tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording request count metric");
        }
    }

    public void RecordActiveRequests(string serviceName, int delta)
    {
        try
        {
            var tags = new TagList { { "service", serviceName } };
            _activeRequests.Add(delta, tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording active requests metric");
        }
    }

    public void RecordRateLimitExceeded(string userId)
    {
        try
        {
            var tags = new TagList { { "user_id", userId } };
            _rateLimitExceeded.Add(1, tags);

            _logger.LogWarning(
                "Rate limit exceeded for user {UserId}",
                userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording rate limit metric");
        }
    }

    public void RecordAuthenticationFailure(string reason)
    {
        try
        {
            var tags = new TagList { { "reason", reason } };
            _authenticationFailures.Add(1, tags);

            _logger.LogWarning(
                "Authentication failure recorded - Reason: {Reason}",
                reason);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording authentication failure metric");
        }
    }

    public void RecordCacheHit(string cacheKey)
    {
        try
        {
            var tags = new TagList { { "cache_key", cacheKey } };
            _cacheHits.Add(1, tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording cache hit metric");
        }
    }

    public void RecordCacheMiss(string cacheKey)
    {
        try
        {
            var tags = new TagList { { "cache_key", cacheKey } };
            _cacheMisses.Add(1, tags);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording cache miss metric");
        }
    }

    public void RecordCircuitBreakerStateChange(string serviceName, string state)
    {
        try
        {
            var delta = state == "Open" ? 1 : -1;
            var tags = new TagList { { "service", serviceName } };
            _circuitBreakerOpen.Add(delta, tags);

            _logger.LogWarning(
                "Circuit breaker state changed - Service: {ServiceName}, State: {State}",
                serviceName, state);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording circuit breaker metric");
        }
    }

    private static string GetStatusClass(int statusCode)
    {
        return statusCode switch
        {
            >= 100 and < 200 => "1xx",
            >= 200 and < 300 => "2xx",
            >= 300 and < 400 => "3xx",
            >= 400 and < 500 => "4xx",
            >= 500 and < 600 => "5xx",
            _ => "unknown"
        };
    }
}
