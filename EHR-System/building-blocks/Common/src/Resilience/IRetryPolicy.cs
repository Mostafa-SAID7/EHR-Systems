using System;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.Common.Resilience;

/// <summary>
/// Interface for retry policy abstraction.
/// Single responsibility: Define retry behavior for transient failures.
/// </summary>
public interface IRetryPolicy
{
    /// <summary>
    /// Execute action with retry policy.
    /// </summary>
    Task ExecuteAsync(Func<Task> action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute action with retry policy and return result.
    /// </summary>
    Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get maximum retry attempts.
    /// </summary>
    int MaxRetries { get; }

    /// <summary>
    /// Get initial delay in milliseconds.
    /// </summary>
    int InitialDelayMs { get; }
}
