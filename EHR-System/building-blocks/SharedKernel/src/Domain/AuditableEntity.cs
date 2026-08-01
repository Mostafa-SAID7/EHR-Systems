using System;

namespace EHRPlatform.SharedKernel.Domain;

/// <summary>
/// Auditable entity class - extends BaseEntity with audit trail support.
/// Tracks creation, updates, and deletion with user information.
/// Separates audit concerns from core entity functionality.
/// </summary>
public abstract class AuditableEntity : BaseEntity, IAuditableEntity
{
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
    /// Initialize auditable entity with creation audit info.
    /// </summary>
    protected AuditableEntity()
    {
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Mark entity as updated by user.
    /// </summary>
    public virtual void MarkAsUpdated(string updatedBy)
    {
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }
}
