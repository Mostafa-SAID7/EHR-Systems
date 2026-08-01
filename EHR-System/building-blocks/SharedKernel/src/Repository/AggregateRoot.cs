using EHRPlatform.SharedKernel.Domain;
using EHRPlatform.SharedKernel.Domain.Events;

namespace EHRPlatform.SharedKernel.Repository;

/// <summary>
/// Base class for aggregate root entities in domain-driven design.
/// Single responsibility: Aggregate root marker and base implementation.
/// </summary>
public abstract class AggregateRoot : AuditableEntity, IAggregateRoot
{
    /// <summary>
    /// Collection of domain events raised by this aggregate.
    /// </summary>
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Get uncleared domain events.
    /// </summary>
    public IReadOnlyList<IDomainEvent> GetUncommittedEvents() => _domainEvents.AsReadOnly();

    /// <summary>
    /// Mark events as committed (alias for ClearUncommittedEvents).
    /// </summary>
    public void MarkEventsAsCommitted() => _domainEvents.Clear();

    /// <summary>
    /// Clear domain events after publishing.
    /// </summary>
    public void ClearUncommittedEvents() => _domainEvents.Clear();

    /// <summary>
    /// Clear all events.
    /// </summary>
    public void ClearEvents() => _domainEvents.Clear();

    /// <summary>
    /// Raise a domain event.
    /// </summary>
    protected void RaiseDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    protected AggregateRoot()
    {
    }

    protected AggregateRoot(Guid id) : base(id)
    {
    }
}
