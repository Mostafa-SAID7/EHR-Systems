using System;

namespace EHRPlatform.Contracts.Dto;

/// <summary>
/// Base DTO for all data transfer objects.
/// Single responsibility: Audit trail information in responses.
/// </summary>
public abstract class BaseDto
{
    /// <summary>
    /// Unique identifier.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// UTC timestamp when resource was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// User who created the resource.
    /// </summary>
    public string CreatedBy { get; set; } = null!;

    /// <summary>
    /// UTC timestamp when resource was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// User who last updated the resource.
    /// </summary>
    public string? UpdatedBy { get; set; }

    /// <summary>
    /// Whether resource is soft-deleted.
    /// </summary>
    public bool IsDeleted { get; set; }
}
