#nullable enable

namespace EHRPlatform.Common.Shared.DTOs;

/// <summary>
/// Response with category metadata for UI rendering.
/// </summary>
public record CategoryMetadataResponse
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
    /// Human-readable description.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Available categories with display names.
    /// </summary>
    public required Dictionary<string, string> AvailableCategories { get; init; }

    /// <summary>
    /// Tags available per category.
    /// </summary>
    public required Dictionary<string, IEnumerable<TagDto>> TagsByCategory { get; init; }

    /// <summary>
    /// Whether multiple tags per category are allowed.
    /// </summary>
    public required bool AllowMultipleTagsPerCategory { get; init; }

    /// <summary>
    /// Whether auto-tagging is enabled.
    /// </summary>
    public required bool EnableAutoTagging { get; init; }

    /// <summary>
    /// Maximum tags per resource (null = unlimited).
    /// </summary>
    public int? MaxTagsPerResource { get; init; }

    /// <summary>
    /// Icon/emoji for UI display.
    /// </summary>
    public string? IconEmoji { get; init; }
}
