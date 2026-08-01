using System;

namespace EHRPlatform.Common.Resilience;

/// <summary>
/// Circuit breaker statistics data structure.
/// Single responsibility: Circuit breaker statistics.
/// </summary>
public class CircuitBreakerStats
{
    /// <summary>
    /// Current state.
    /// </summary>
    public CircuitBreakerState State { get; set; }

    /// <summary>
    /// Total requests processed.
    /// </summary>
    public long TotalRequests { get; set; }

    /// <summary>
    /// Successful requests.
    /// </summary>
    public long SuccessfulRequests { get; set; }

    /// <summary>
    /// Failed requests.
    /// </summary>
    public long FailedRequests { get; set; }

    /// <summary>
    /// Requests rejected due to open circuit.
    /// </summary>
    public long RejectedRequests { get; set; }

    /// <summary>
    /// Last failure time.
    /// </summary>
    public DateTime? LastFailureTime { get; set; }
}
