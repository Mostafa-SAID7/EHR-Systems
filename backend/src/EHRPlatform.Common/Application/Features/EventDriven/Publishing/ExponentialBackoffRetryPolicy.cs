namespace EHRPlatform.Common.Application.Features.EventDriven.Publishing;

/// <summary>
/// Implements exponential backoff retry strategy.
/// Single responsibility: Retry logic only.
/// </summary>
public class ExponentialBackoffRetryPolicy : IRetryPolicy
{
    private const int BaseDelaySeconds = 2;
    private const double BackoffMultiplier = 2.0;

    public bool ShouldRetry(int attemptCount, int maxAttempts)
    {
        return attemptCount < maxAttempts;
    }

    public TimeSpan GetRetryDelay(int attemptCount)
    {
        // Exponential: 2s, 4s, 8s, 16s, etc.
        var delaySeconds = BaseDelaySeconds * Math.Pow(BackoffMultiplier, attemptCount);
        return TimeSpan.FromSeconds(Math.Min(delaySeconds, 300)); // Cap at 5 minutes
    }
}
