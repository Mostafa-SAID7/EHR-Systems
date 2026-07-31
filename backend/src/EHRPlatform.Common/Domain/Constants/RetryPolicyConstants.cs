#nullable enable

namespace EHRPlatform.Common.Domain.Constants;

/// <summary>
/// Retry policy and backoff configuration constants.
/// Defines exponential backoff parameters for transient failures.
/// Single responsibility: Define retry policy constants only.
/// </summary>
public static class RetryPolicyConstants
{
    /// <summary>Base delay in seconds for exponential backoff.</summary>
    public const int BaseDelaySeconds = 2;

    /// <summary>Backoff multiplier for exponential delay calculation.</summary>
    public const double BackoffMultiplier = 2.0;

    /// <summary>Maximum number of retry attempts.</summary>
    public const int MaxRetryAttempts = 5;

    /// <summary>Maximum delay in seconds between retries (cap for exponential backoff).</summary>
    public const int MaxDelaySeconds = 300; // 5 minutes

    /// <summary>Jitter factor (0.0 to 1.0) to randomize delays and prevent thundering herd.</summary>
    public const double JitterFactor = 0.1;
}
