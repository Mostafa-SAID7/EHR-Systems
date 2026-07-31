using EHRPlatform.Common.Domain.Events;
using EHRPlatform.Common.Domain.Entities;
using System.Text.Json;
using EHRPlatform.Common.Shared.Utilities.Helpers;
using Microsoft.EntityFrameworkCore;

namespace EHRPlatform.Common.Application.Features.EventDriven.Outbox;

/// <summary>
/// Builds outbox events from domain events.
/// Single responsibility: Transform domain events into OutboxEvent records only.
/// </summary>
public class OutboxEventBuilder
{
    /// <summary>
    /// Build outbox events from domain events collected from change tracker.
    /// </summary>
    public List<OutboxEvent> BuildFromDomainEvents(IEnumerable<DomainEvent> domainEvents)
    {
        return domainEvents
            .Select(de => BuildOutboxEvent(de))
            .ToList();
    }

    /// <summary>
    /// Build a single outbox event from a domain event.
    /// </summary>
    private OutboxEvent BuildOutboxEvent(DomainEvent domainEvent)
    {
        var eventType = domainEvent.GetType();

        return new OutboxEvent
        {
            Id = GuidHelper.NewGuid(),
            EventType = eventType.FullName ?? eventType.Name,
            EventData = JsonSerializationHelper.Serialize(domainEvent, eventType),
            CreatedAt = DateTimeHelper.UtcNow,
            IsPublished = false,
            PublishedAt = null,
            PublishAttempts = 0,
            ErrorMessage = null
        };
    }

    /// <summary>
    /// Collect and clear domain events from all entities in change tracker.
    /// </summary>
    public List<DomainEvent> CollectAndClearDomainEvents(DbContext context)
    {
        var domainEvents = context.ChangeTracker
            .Entries<BaseEntity>()
            .SelectMany(e => e.Entity.GetDomainEvents())
            .ToList();

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            entry.Entity.ClearDomainEvents();
        }

        return domainEvents;
    }
}
