namespace EHRPlatform.Common.Application.Features.EventDriven.Publishing;

/// <summary>
/// Abstraction for retry policies.
/// Single responsibility: Define retry contract.
/// </summary>
public interface IRetryPolicy
{
    /// <summary>
    /// Determine if an event should be retried based on attempt count.
    /// </summary>
    bool ShouldRetry(int attemptCount, int maxAttempts);

    /// <summary>
    /// Get delay before next retry attempt.
    /// </summary>
    TimeSpan GetRetryDelay(int attemptCount);
}
