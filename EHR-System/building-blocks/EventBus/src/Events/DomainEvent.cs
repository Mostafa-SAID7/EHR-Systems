using System;
using MediatR;

namespace EHRPlatform.EventBus.Events;

/// <summary>
/// Base class for domain events.
/// Domain events represent something that happened in the business domain.
/// 
/// Example: PatientCreatedDomainEvent, AppointmentScheduledDomainEvent
/// 
/// Domain events are published and consumed within the same service.
/// For inter-service communication, use IntegrationEvent instead.
/// </summary>
public abstract class DomainEvent : INotification
{
    /// <summary>
    /// Event ID (unique identifier for this event instance).
    /// </summary>
    public Guid EventId { get; protected set; } = Guid.NewGuid();

    /// <summary>
    /// Aggregate ID (the root entity this event belongs to).
    /// </summary>
    public Guid AggregateId { get; protected set; }

    /// <summary>
    /// Aggregate type (e.g., "Patient", "Appointment").
    /// </summary>
    public string AggregateType { get; protected set; } = null!;

    /// <summary>
    /// When the event occurred.
    /// </summary>
    public DateTime OccurredAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Correlation ID for tracing.
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
}
