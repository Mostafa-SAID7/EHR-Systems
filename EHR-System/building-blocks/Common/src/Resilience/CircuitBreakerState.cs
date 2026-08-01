namespace EHRPlatform.Common.Resilience;

/// <summary>
/// Circuit breaker state enumeration.
/// Single responsibility: Circuit breaker state values.
/// </summary>
public enum CircuitBreakerState
{
    /// <summary>
    /// Circuit is closed - requests proceed normally.
    /// </summary>
    Closed = 0,

    /// <summary>
    /// Circuit is open - requests fail immediately.
    /// </summary>
    Open = 1,

    /// <summary>
    /// Circuit is half-open - testing if service recovered.
    /// </summary>
    HalfOpen = 2
}
