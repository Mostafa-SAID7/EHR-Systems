#nullable enable

namespace EHRPlatform.Common.Tags;

/// <summary>
/// Tag response DTO for API responses.
/// </summary>
public class TagDto
{
    /// <summary>Tag ID.</summary>
    public Guid Id { get; set; }

    /// <summary>Tag name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>URL-friendly slug.</summary>
    public string Slug { get; set; } = string.Empty;

    /// <summary>Tag category.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Description.</summary>
    public string? Description { get; set; }

    /// <summary>Color code for UI.</summary>
    public string? ColorCode { get; set; }

    /// <summary>Whether tag is archived.</summary>
    public bool IsArchived { get; set; }

    /// <summary>Usage count.</summary>
    public int UsageCount { get; set; }

    /// <summary>Whether tag is system-managed.</summary>
    public bool IsSystemTag { get; set; }

    /// <summary>Allowed services.</summary>
    public string? AllowedServices { get; set; }

    /// <summary>Creation timestamp.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>Last modification timestamp.</summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Tag association DTO for API responses.
/// </summary>
public class TagAssociationDto
{
    /// <summary>Association ID.</summary>
    public Guid Id { get; set; }

    /// <summary>Tag information.</summary>
    public TagDto? Tag { get; set; }

    /// <summary>Resource ID.</summary>
    public Guid ResourceId { get; set; }

    /// <summary>Resource type.</summary>
    public string ResourceType { get; set; } = string.Empty;

    /// <summary>Context/relationship description.</summary>
    public string? Context { get; set; }

    /// <summary>Service name.</summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>When tag was applied.</summary>
    public DateTime AppliedAt { get; set; }

    /// <summary>Who applied the tag.</summary>
    public string? AppliedBy { get; set; }
}

/// <summary>
/// Request DTO for creating or updating tags.
/// </summary>
public class CreateOrUpdateTagRequest
{
    /// <summary>Tag name.</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Tag category.</summary>
    public string Category { get; set; } = string.Empty;

    /// <summary>Description.</summary>
    public string? Description { get; set; }

    /// <summary>Color code.</summary>
    public string? ColorCode { get; set; }

    /// <summary>Allowed services (comma-separated).</summary>
    public string? AllowedServices { get; set; }
}
