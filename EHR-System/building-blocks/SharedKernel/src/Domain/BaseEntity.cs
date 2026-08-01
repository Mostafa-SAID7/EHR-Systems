using System;

namespace EHRPlatform.SharedKernel.Domain;

/// <summary>
/// Base entity class for all domain entities.
/// Provides common audit fields: CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy.
/// Supports soft deletes and correlation tracking for distributed tracing.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier (GUID).
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// UTC timestamp when entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// User ID (or system) that created the entity.
    /// </summary>
    public string CreatedBy { get; set; } = null!;

    /// <summary>
    /// UTC timestamp when entity was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// User ID (or system) that last updated the entity.
    /// Null if entity has not been updated since creation.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// UTC timestamp when entity was soft-deleted.
    /// Null if entity is not deleted.
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// User ID (or system) that deleted the entity.
    /// Null if entity is not deleted.
    /// </summary>
    public string? DeletedBy { get; set; }

    /// <summary>
    /// Correlation ID for distributed tracing (e.g., request ID).
    /// Links entity changes across microservices.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Whether entity is soft-deleted.
    /// </summary>
    public bool IsDeleted => DeletedAt.HasValue;

    /// <summary>
    /// Initialize entity with new ID, creation timestamp, and creator.
    /// </summary>
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark entity as updated.
    /// </summary>
    public virtual void MarkAsUpdated(string updatedBy)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Soft delete entity.
    /// </summary>
    public virtual void Delete(string deletedBy)
    {
        DeletedAt = DateTime.UtcNow;
        DeletedBy = deletedBy;
    }

    /// <summary>
    /// Restore soft-deleted entity.
    /// </summary>
    public virtual void Restore()
    {
        DeletedAt = null;
        DeletedBy = null;
    }

    /// <summary>
    /// Set correlation ID for tracing.
    /// </summary>
    public void SetCorrelationId(string correlationId)
    {
        CorrelationId = correlationId;
    }
}
