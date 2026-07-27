#nullable enable

using EHRPlatform.Common.Tags;

namespace EHRPlatform.Common.Categories;

/// <summary>
/// Provider for centralized categorization across services.
/// Each service implements this to define which categories and tags are relevant.
/// Eliminates duplicate categorization logic across 10+ services.
/// </summary>
public interface ICategoryProvider
{
    /// <summary>
    /// Get service name this provider belongs to.
    /// </summary>
    string ServiceName { get; }

    /// <summary>
    /// Get resource type this provider manages.
    /// Examples: "Patient", "Appointment", "Invoice", "ClinicalNote"
    /// </summary>
    string ResourceType { get; }

    /// <summary>
    /// Get all category slugs available for this resource type.
    /// Returns slugs like "priority", "status", "workflow", "classification"
    /// </summary>
    IEnumerable<string> GetAvailableCategories();

    /// <summary>
    /// Get predefined tags for a specific category.
    /// For example, Priority category might return ["urgent", "high", "medium", "low"]
    /// </summary>
    Task<IEnumerable<Tag>> GetCategoryTagsAsync(
        string categorySlug,
        ITagService tagService,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get recommended tags for a specific resource based on its properties.
    /// Implements service-specific business logic to suggest appropriate tags.
    /// </summary>
    Task<IEnumerable<Guid>> GetRecommendedTagIdsAsync(
        Guid resourceId,
        object resourceData,
        ITagService tagService,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Validate that a tag can be applied to this resource type in this service.
    /// </summary>
    Task<bool> CanApplyTagAsync(
        Guid resourceId,
        Guid tagId,
        ITagService tagService,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Get categorization metadata (display names, descriptions, UI hints).
    /// Helps UI render category selectors and tag pickers.
    /// </summary>
    CategoryMetadata GetMetadata();

    /// <summary>
    /// Get tags that should be automatically applied to newly created resources.
    /// </summary>
    Task<IEnumerable<Guid>> GetDefaultTagsAsync(
        ITagService tagService,
        CancellationToken cancellationToken = default);
}

/// <summary>
/// Metadata about a service's categorization capabilities.
/// Used by UI to render category/tag controls intelligently.
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
