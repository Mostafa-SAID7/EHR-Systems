using System;

namespace EHRPlatform.SharedKernel.Domain;

/// <summary>
/// Auditable entity interface - tracks creation and modifications.
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
    /// User that last updated entity.
    /// </summary>
    string? UpdatedBy { get; }

    /// <summary>
    /// When entity was deleted (soft delete).
    /// </summary>
    DateTime? DeletedAt { get; }

    /// <summary>
    /// User that deleted entity.
    /// </summary>
    string? DeletedBy { get; }
}
