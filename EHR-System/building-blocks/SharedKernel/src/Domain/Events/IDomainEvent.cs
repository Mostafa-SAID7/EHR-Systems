using System;

namespace EHRPlatform.SharedKernel.Domain.Events;

/// <summary>
/// Domain event contract - represents something that happened in the domain.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Aggregate that raised this event.
    /// </summary>
    Guid AggregateId { get; }

    /// <summary>
    /// Name of aggregate type.
    /// </summary>
    string AggregateName { get; }

    /// <summary>
    /// When event occurred.
    /// </summary>
    DateTime OccurredAt { get; }

    /// <summary>
    /// Correlation ID for distributed tracing.
    /// </summary>
    string CorrelationId { get; }

    /// <summary>
    /// User ID that triggered the event.
    /// </summary>
    string UserId { get; }
}
