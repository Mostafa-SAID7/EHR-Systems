#nullable enable

namespace EHRPlatform.Common.Shared.DTOs;

/// <summary>
/// Metadata about a service's categorization capabilities.
/// Used by UI to render category/tag controls intelligently.
/// Single responsibility: Store category metadata only.
/// </summary>
public record CategoryMetadata
{
    /// <summary>
    /// Service name.
    /// </summary>
    public required string ServiceName { get; init; }

    /// <summary>
    /// Resource type name.
    /// </summary>
    public required string ResourceType { get; init; }

    /// <summary>
    /// Human-readable description of this resource type.
    /// Example: "Medical appointments scheduled for patients"
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Available categories and their metadata.
    /// Key: category slug, Value: category display name
    /// </summary>
    public required Dictionary<string, string> AvailableCategories { get; init; }

    /// <summary>
    /// Whether multiple tags from same category can be applied.
    /// Default: true. Set false for single-select categories like "Status"
    /// </summary>
    public bool AllowMultipleTagsPerCategory { get; init; } = true;

    /// <summary>
    /// Whether auto-tagging is enabled for this resource type.
    /// </summary>
    public bool EnableAutoTagging { get; init; } = false;

    /// <summary>
    /// Maximum number of tags that can be applied per resource.
    /// Null = no limit.
    /// </summary>
    public int? MaxTagsPerResource { get; init; }

    /// <summary>
    /// Icon/emoji for UI display.
    /// </summary>
    public string? IconEmoji { get; init; }
}
