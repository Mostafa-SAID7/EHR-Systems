namespace EHRPlatform.Common.Resilience;

/// <summary>
/// Backoff strategy enumeration.
/// Single responsibility: Backoff strategy type values.
/// </summary>
public enum BackoffStrategyType
{
    /// <summary>
    /// No delay between retries.
    /// </summary>
    Immediate = 0,

    /// <summary>
    /// Fixed delay between retries.
    /// </summary>
    Linear = 1,

    /// <summary>
    /// Exponentially increasing delay.
    /// </summary>
    Exponential = 2,

    /// <summary>
    /// Exponential with random jitter.
    /// </summary>
    ExponentialWithJitter = 3
}
