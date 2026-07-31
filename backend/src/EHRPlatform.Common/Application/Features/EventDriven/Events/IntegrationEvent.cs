#nullable enable

using EHRPlatform.Common.Domain.Events;

namespace EHRPlatform.Common.Application.Features.EventDriven.Events;

/// <summary>
/// Concrete integration event used internally by the outbox processor when
/// re-publishing stored events whose original type is not available at runtime.
/// Single responsibility: Provide concrete wrapper for generic event republishing.
/// </summary>
internal sealed class OutboxPublishableEvent : IntegrationEvent
{
    private readonly string _eventType;

    /// <summary>
    /// Create an outbox publishable event wrapper.
    /// </summary>
    public OutboxPublishableEvent(Guid eventId, string eventType, DateTime occurredAt)
    {
        EventId = eventId;
        _eventType = eventType;
        OccurredAt = occurredAt;
    }

    /// <summary>
    /// Override EventType with the original event type name.
    /// </summary>
    public override string EventType => _eventType;
}
