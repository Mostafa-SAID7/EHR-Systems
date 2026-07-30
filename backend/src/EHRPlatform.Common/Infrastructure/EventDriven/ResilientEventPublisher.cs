using EHRPlatform.Common.Events;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;

namespace EHRPlatform.Common.Messaging;

/// <summary>
/// Polly-resilient decorator around <see cref="IEventPublisher"/>.
/// Adds retry with exponential back-off and a circuit breaker for Kafka publish operations.
///
/// Retry policy: 3 attempts with 2^n second delays (2s, 4s, 8s).
/// Circuit breaker: opens after 5 consecutive failures, recovers after 30s.
///
/// HIPAA: All publish failures are logged with event ID for audit tracing.
/// </summary>
public sealed class ResilientEventPublisher : IEventPublisher
{
    private readonly IEventPublisher _inner;
    private readonly ILogger<ResilientEventPublisher> _logger;
    private readonly AsyncRetryPolicy _retryPolicy;
    private readonly AsyncCircuitBreakerPolicy _circuitBreaker;

    public ResilientEventPublisher(IEventPublisher inner, ILogger<ResilientEventPublisher> logger)
    {
        _inner = inner ?? throw new ArgumentNullException(nameof(inner));
        _logger = logger;

        _retryPolicy = Policy
            .Handle<Exception>(ex => ex is not OperationCanceledException)
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: attempt => TimeSpan.FromSeconds(Math.Pow(2, attempt)),
                onRetry: (exception, delay, attempt, context) =>
                {
                    _logger.LogWarning(
                        exception,
                        "Event publish retry {Attempt}/3 after {Delay}s. EventId={EventId}",
                        attempt, delay.TotalSeconds,
                        context.ContainsKey("EventId") ? context["EventId"] : "unknown");
                });

        _circuitBreaker = Policy
            .Handle<Exception>(ex => ex is not OperationCanceledException)
            .CircuitBreakerAsync(
                exceptionsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(30),
                onBreak: (exception, duration) =>
                    _logger.LogError(
                        exception,
                        "Kafka circuit breaker OPEN for {Duration}s – event publishing suspended",
                        duration.TotalSeconds),
                onReset: () =>
                    _logger.LogInformation("Kafka circuit breaker CLOSED – event publishing resumed"),
                onHalfOpen: () =>
                    _logger.LogInformation("Kafka circuit breaker HALF-OPEN – probing Kafka"));
    }

    /// <inheritdoc/>
    public Task PublishAsync(IntegrationEvent @event, CancellationToken cancellationToken = default)
    {
        var context = new Context { ["EventId"] = @event.EventId.ToString() };

        return Policy.WrapAsync(_retryPolicy, _circuitBreaker)
            .ExecuteAsync(
                (ctx, ct) => _inner.PublishAsync(@event, ct),
                context,
                cancellationToken);
    }

    /// <inheritdoc/>
    public async Task PublishBatchAsync(IEnumerable<IntegrationEvent> events, CancellationToken cancellationToken = default)
    {
        var eventList = events.ToList();
        var tasks = eventList.Select(e => PublishAsync(e, cancellationToken));
        await Task.WhenAll(tasks);
    }
}
