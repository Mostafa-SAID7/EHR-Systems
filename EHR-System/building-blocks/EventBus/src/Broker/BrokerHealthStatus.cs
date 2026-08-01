namespace EHRPlatform.EventBus.Broker;

/// <summary>
/// Broker health status enumeration.
/// Single responsibility: Message broker health status values.
/// </summary>
public enum BrokerHealthStatus
{
    /// <summary>
    /// Broker is healthy and operational.
    /// </summary>
    Healthy = 0,

    /// <summary>
    /// Broker is degraded but operational.
    /// </summary>
    Degraded = 1,

    /// <summary>
    /// Broker is unhealthy.
    /// </summary>
    Unhealthy = 2,

    /// <summary>
    /// Broker connection status unknown.
    /// </summary>
    Unknown = 3
}
