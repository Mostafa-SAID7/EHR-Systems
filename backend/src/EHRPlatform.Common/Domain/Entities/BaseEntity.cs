#nullable enable

using EHRPlatform.Common.Domain.Events;
using EHRPlatform.Common.Shared.Utilities.Helpers;

namespace EHRPlatform.Common.Domain.Entities;

/// <summary>
/// Base entity for all domain entities in the system.
/// Provides standard properties for all entities.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity.
    /// </summary>
    public Guid Id { get; set; } = GuidHelper.NewGuid();

    /// <summary>
    /// When the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTimeHelper.UtcNow;

    /// <summary>
    /// User ID who created the entity.
    /// </summary>
    public Guid CreatedBy { get; set; }

    /// <summary>
    /// When the entity was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTimeHelper.UtcNow;

    /// <summary>
    /// User ID who last updated the entity.
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// Soft delete timestamp. Null if not deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// User ID who deleted the entity.
    /// </summary>
    public Guid? DeletedBy { get; set; }

    /// <summary>
    /// Indicates if the entity is deleted (soft delete).
    /// </summary>
    public bool IsDeleted => DeletedAt.HasValue;

    /// <summary>
    /// Correlation ID for tracking changes across systems.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Domain events raised by this entity (not persisted).
    /// </summary>
    private readonly List<DomainEvent> _domainEvents = new();

    /// <summary>
    /// Gets the collection of domain events.
    /// </summary>
    public IReadOnlyCollection<DomainEvent> GetDomainEvents() => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to the entity.
    /// </summary>
    public void RaiseDomainEvent(DomainEvent @event)
    {
        _domainEvents.Add(@event);
    }

    /// <summary>
    /// Clears all domain events from the entity.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

