using System;

namespace EHRPlatform.SharedKernel.Domain;

/// <summary>
/// Auditable entity interface - tracks creation and modifications.
/// Separated from core entity to support optional audit trails.
/// </summary>
public interface IAuditableEntity : IEntity
{
    /// <summary>
    /// When entity was created.
    /// </summary>
    DateTime CreatedAt { get; }

    /// <summary>
    /// User that created entity.
    /// </summary>
    string CreatedBy { get; }

    /// <summary>
    /// When entity was last updated.
    /// </summary>
    DateTime UpdatedAt { get; }

    /// <summary>
    /// User that last updated entity (null if never updated).
    /// </summary>
    string? UpdatedBy { get; }

    /// <summary>
    /// Mark entity as updated.
    /// </summary>
    void MarkAsUpdated(string updatedBy);
}
