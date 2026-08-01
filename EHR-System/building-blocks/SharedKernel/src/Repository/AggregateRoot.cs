using EHRPlatform.SharedKernel.Domain;

namespace EHRPlatform.SharedKernel.Repository;

/// <summary>
/// Base class for aggregate root entities in domain-driven design.
/// Single responsibility: Aggregate root marker and base implementation.
/// </summary>
public abstract class AggregateRoot : AuditableEntity
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
    /// Clear domain events after publishing.
    /// </summary>
    public void ClearUncommittedEvents() => _domainEvents.Clear();

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

/// <summary>
/// Marker interface for domain events.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// Event occurrence date/time.
    /// </summary>
    DateTime OccurredAt { get; }

    /// <summary>
    /// Event ID for tracking.
    /// </summary>
    Guid EventId { get; }
}
