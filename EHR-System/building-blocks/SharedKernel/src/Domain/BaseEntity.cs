using System;

namespace EHRPlatform.SharedKernel.Domain;

/// <summary>
/// Base entity class.
/// Single responsibility: Base entity functionality.
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Entity ID.
    /// </summary>
    public string Id { get; protected set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// Created timestamp.
    /// </summary>
    public DateTime CreatedAt { get; protected set; } = DateTime.UtcNow;

    /// <summary>
    /// Created by user ID.
    /// </summary>
    public string? CreatedBy { get; protected set; }

    /// <summary>
    /// Last modified timestamp.
    /// </summary>
    public DateTime? ModifiedAt { get; protected set; }

    /// <summary>
    /// Last modified by user ID.
    /// </summary>
    public string? ModifiedBy { get; protected set; }

    /// <summary>
    /// Is entity deleted (soft delete).
    /// </summary>
    public bool IsDeleted { get; protected set; }

    /// <summary>
    /// Deleted timestamp.
    /// </summary>
    public DateTime? DeletedAt { get; protected set; }
}
