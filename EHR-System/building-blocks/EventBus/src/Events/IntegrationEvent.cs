using System;

namespace EHRPlatform.EventBus.Events;

/// <summary>
/// Base class for integration events.
/// Single responsibility: Integration event base.
/// </summary>
public abstract class IntegrationEvent
{
    /// <summary>
    /// Event ID.
    /// </summary>
    public string Id { get; protected set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Event creation timestamp.
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Correlation ID for distributed tracing.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Causation ID (what caused this event).
    /// </summary>
    public string? CausationId { get; set; }

    /// <summary>
    /// User ID who triggered the event.
    /// </summary>
    public string? UserId { get; set; }
}
