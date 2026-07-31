#nullable enable

namespace EHRPlatform.Common.Shared.DTOs;

/// <summary>
/// Base response DTO that includes slug-friendly URL representation.
/// Inherit from this to provide consistent slug support across all response models.
/// </summary>
public abstract class SluggedResponseDto
{
    /// <summary>
    /// Unique identifier (GUID).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// URL-friendly slug representation of the entity.
    /// For backward compatibility, both GUID and slug URLs should be supported.
    /// </summary>
    public string? Slug { get; set; }

    /// <summary>
    /// Display name for the slug (readable version).
    /// Useful for UI breadcrumbs and navigation.
    /// </summary>
    public string? SlugDisplayName { get; set; }
}

