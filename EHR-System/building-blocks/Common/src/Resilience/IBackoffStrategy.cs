namespace EHRPlatform.Common.Resilience;

/// <summary>
/// Interface for backoff strategy (how to calculate delay between retries).
/// Single responsibility: Calculate retry delay.
/// </summary>
public interface IBackoffStrategy
{
    /// <summary>
    /// Calculate delay in milliseconds for retry attempt.
    /// </summary>
    int GetDelay(int attemptNumber);

    /// <summary>
    /// Get strategy name.
    /// </summary>
    string Name { get; }
}
