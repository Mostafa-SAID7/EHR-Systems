using System;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.Common.Resilience;

/// <summary>
/// Interface for circuit breaker pattern (prevent cascading failures).
/// Single responsibility: Protect against failure avalanche.
/// </summary>
public interface ICircuitBreaker
{
    /// <summary>
    /// Execute action within circuit breaker.
    /// </summary>
    Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute action within circuit breaker with result.
    /// </summary>
    Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get current circuit breaker state.
    /// </summary>
    CircuitBreakerState State { get; }

    /// <summary>
    /// Get failure count in current window.
    /// </summary>
    int FailureCount { get; }

    /// <summary>
    /// Reset circuit breaker.
    /// </summary>
    Task ResetAsync();

    /// <summary>
    /// Get circuit breaker statistics.
    /// </summary>
    Task<CircuitBreakerStats> GetStatsAsync();
}
