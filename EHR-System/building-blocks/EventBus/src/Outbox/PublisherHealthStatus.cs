namespace EHRPlatform.EventBus.Outbox;

/// <summary>
/// Publisher health status enumeration.
/// Single responsibility: Health status values.
/// </summary>
public enum PublisherHealthStatus
{
    /// <summary>
    /// Publisher is healthy.
    /// </summary>
    Healthy = 0,

    /// <summary>
    /// Publisher is degraded.
    /// </summary>
    Degraded = 1,

    /// <summary>
    /// Publisher is unhealthy.
    /// </summary>
    Unhealthy = 2
}
