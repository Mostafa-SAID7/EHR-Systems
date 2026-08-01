using System.Collections.Generic;

namespace EHRPlatform.SharedKernel.Domain;

/// <summary>
/// Aggregate root interface - entity that manages domain events.
/// Single responsibility: Aggregate root contract.
/// </summary>
public interface IAggregateRoot : IEntity
{
    /// <summary>
    /// Get uncommitted domain events.
    /// </summary>
    IReadOnlyList<IDomainEvent> GetUncommittedEvents();

    /// <summary>
    /// Mark events as committed.
    /// </summary>
    void MarkEventsAsCommitted();

    /// <summary>
    /// Clear all events.
    /// </summary>
    void ClearEvents();
}
