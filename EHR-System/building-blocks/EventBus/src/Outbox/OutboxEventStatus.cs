namespace EHRPlatform.EventBus.Outbox;

/// <summary>
/// Outbox event status enumeration.
/// Single responsibility: Event status values.
/// </summary>
public enum OutboxEventStatus
{
    /// <summary>
    /// Event is pending publication.
    /// </summary>
    Pending = 0,

    /// <summary>
    /// Event has been published.
    /// </summary>
    Published = 1,

    /// <summary>
    /// Event publication failed.
    /// </summary>
    Failed = 2
}
