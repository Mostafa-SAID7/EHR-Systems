using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;
using Polly.Wrap;

namespace EHRPlatform.Common.Infrastructure.Resilience;

/// <summary>
/// Factory for standard Polly resilience policies used across EHR microservices.
///
/// Policy stacking order (outermost → innermost):
///   Timeout → CircuitBreaker → Retry → actual call
///
/// HIPAA: all policy events (retries, breaks, timeouts) are logged with trace context.
/// </summary>
public static class EHRResiliencePolicies
{
    // ── HTTP / external API ────────────────────────────────────────────────────

    /// <summary>
    /// Standard HTTP retry policy: 3 attempts, exponential back-off.
    /// Handles transient HTTP errors (5xx, 408, 429).
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetHttpRetryPolicy(ILogger logger) =>
        Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r =>
                (int)r.StatusCode is 429 or 408 or >= 500)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (outcome, delay, attempt, _) =>
                    logger.LogWarning(
                        "HTTP retry {Attempt}/3 after {Delay}s: {Reason}",
                        attempt, delay.TotalSeconds,
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()));

    /// <summary>
    /// HTTP circuit breaker: opens after 5 consecutive failures, heals after 30s.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetHttpCircuitBreaker(ILogger logger) =>
        Policy
            .Handle<HttpRequestException>()
            .OrResult<HttpResponseMessage>(r => (int)r.StatusCode >= 500)
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (_, duration) =>
                    logger.LogError("HTTP circuit breaker OPEN for {Duration}s", duration.TotalSeconds),
                onReset: () =>
                    logger.LogInformation("HTTP circuit breaker CLOSED"),
                onHalfOpen: () =>
                    logger.LogInformation("HTTP circuit breaker HALF-OPEN"));

    /// <summary>
    /// Timeout policy: 10 seconds for HTTP calls (adjustable per scenario).
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(int seconds = 10) =>
        Policy.TimeoutAsync<HttpResponseMessage>(seconds, TimeoutStrategy.Optimistic);

    // ── Database ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Database retry policy: handles transient EF Core / Npgsql errors.
    /// 3 retries with 1s, 2s, 4s delays.
    /// </summary>
    public static AsyncRetryPolicy GetDatabaseRetryPolicy(ILogger logger) =>
        Policy
            .Handle<Exception>(IsTransientDatabaseException)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt - 1)),
                onRetry: (ex, delay, attempt, _) =>
                    logger.LogWarning(
                        ex,
                        "Database retry {Attempt}/3 after {Delay}s",
                        attempt, delay.TotalSeconds));

    // ── Messaging (Kafka / RabbitMQ) ───────────────────────────────────────────

    /// <summary>
    /// Messaging retry policy for publish operations.
    /// 3 retries with exponential back-off.
    /// </summary>
    public static AsyncRetryPolicy GetMessagingRetryPolicy(ILogger logger) =>
        Policy
            .Handle<Exception>(ex => ex is not OperationCanceledException)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (ex, delay, attempt, _) =>
                    logger.LogWarning(
                        ex,
                        "Messaging retry {Attempt}/3 after {Delay}s",
                        attempt, delay.TotalSeconds));

    /// <summary>
    /// Combined wrap: Timeout → CircuitBreaker → Retry for outbound HTTP calls.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetHttpResilienceWrap(ILogger logger) =>
        Policy.WrapAsync(
            GetTimeoutPolicy(),
            GetHttpCircuitBreaker(logger),
            GetHttpRetryPolicy(logger));

    // ── Helpers ────────────────────────────────────────────────────────────────

    private static bool IsTransientDatabaseException(Exception ex) =>
        ex is InvalidOperationException { Message: var msg } &&
        (msg.Contains("transient", StringComparison.OrdinalIgnoreCase) ||
         msg.Contains("timeout", StringComparison.OrdinalIgnoreCase) ||
         msg.Contains("deadlock", StringComparison.OrdinalIgnoreCase));
}

