using System;

namespace EHRPlatform.EventBus.Events;

/// <summary>
/// Base class for integration events.
/// Integration events are published across service boundaries via message broker.
/// 
/// Example: PatientCreatedIntegrationEvent (published by Patient service, consumed by Notification, Analytics, etc.)
/// 
/// Integration events enable eventual consistency across services.
/// They are persisted in the outbox table and published asynchronously.
/// </summary>
public abstract class IntegrationEvent
{
    /// <summary>
    /// Event ID (unique identifier).
    /// </summary>
    public Guid Id { get; protected set; } = Guid.NewGuid();

    /// <summary>
    /// When the event occurred.
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Correlation ID for distributed tracing.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// User who triggered the event.
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Event version (for schema evolution).
    /// </summary>
    public int Version { get; protected set; } = 1;

    /// <summary>
    /// Event name (automatically derived from class name).
    /// </summary>
    public string EventName => GetType().Name;
}
