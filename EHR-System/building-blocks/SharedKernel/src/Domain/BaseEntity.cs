using System;

namespace EHRPlatform.SharedKernel.Domain;

/// <summary>
/// Base entity class - core entity with ID and deletion support only.
/// Separates core entity concern from audit trails.
/// </summary>
public abstract class BaseEntity : IEntity
{
    /// <summary>
    /// Unique identifier (GUID).
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// Soft delete timestamp.
    /// Null if entity is not deleted.
    /// </summary>
    public DateTime? DeletedAt { get; protected set; }

    /// <summary>
    /// User that deleted entity.
    /// Null if entity is not deleted.
    /// </summary>
    public string? DeletedBy { get; protected set; }

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
    /// Initialize entity with new ID.
    /// </summary>
    protected BaseEntity()
    {
        Id = Guid.NewGuid();
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
