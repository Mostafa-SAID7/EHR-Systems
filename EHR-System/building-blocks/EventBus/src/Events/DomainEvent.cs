using System;

namespace EHRPlatform.EventBus.Events;

/// <summary>
/// Base class for domain events.
/// Single responsibility: Domain event base.
/// </summary>
public abstract class DomainEvent
{
    /// <summary>
    /// Event ID.
    /// </summary>
    public string Id { get; protected set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Aggregate ID that raised the event.
    /// </summary>
    public string AggregateId { get; protected set; } = null!;

    /// <summary>
    /// Event creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Event version/sequence number.
    /// </summary>
    public int Version { get; protected set; }

    /// <summary>
    /// User ID who triggered the event.
    /// </summary>
    public string? UserId { get; set; }
}
