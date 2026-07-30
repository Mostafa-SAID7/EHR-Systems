#nullable enable

namespace EHRPlatform.Common.Shared.DTOs;

/// <summary>
/// Request to apply tags to a resource.
/// </summary>
public record ApplyTagsRequest
{
    /// <summary>
    /// Resource ID to tag.
    /// </summary>
    public required Guid ResourceId { get; init; }

    /// <summary>
    /// Resource type (e.g., "Patient", "Appointment", "Invoice").
    /// </summary>
    public required string ResourceType { get; init; }

    /// <summary>
    /// Tag IDs to apply.
    /// </summary>
    public required IEnumerable<Guid> TagIds { get; init; } = Enumerable.Empty<Guid>();

    /// <summary>
    /// Service name that owns this resource.
    /// </summary>
    public required string ServiceName { get; init; }

    /// <summary>
    /// Optional context about why these tags are being applied.
    /// </summary>
    public string? Context { get; init; }

    /// <summary>
    /// User ID or identifier of who is applying the tags.
    /// </summary>
    public string? AppliedBy { get; init; }
}

/// <summary>
/// Request to remove a tag from a resource.
/// </summary>
public record RemoveTagRequest
{
    /// <summary>
    /// Resource ID.
    /// </summary>
    public required Guid ResourceId { get; init; }

    /// <summary>
    /// Resource type.
    /// </summary>
    public required string ResourceType { get; init; }

    /// <summary>
    /// Tag ID to remove.
    /// </summary>
    public required Guid TagId { get; init; }
}

/// <summary>
/// Request to replace all tags on a resource.
/// </summary>
public record SetResourceTagsRequest
{
    /// <summary>
    /// Resource ID.
    /// </summary>
    public required Guid ResourceId { get; init; }

    /// <summary>
    /// Resource type.
    /// </summary>
    public required string ResourceType { get; init; }

    /// <summary>
    /// Tag IDs to set (replaces all existing tags).
    /// </summary>
    public required IEnumerable<Guid> TagIds { get; init; } = Enumerable.Empty<Guid>();

    /// <summary>
    /// Service name.
    /// </summary>
    public required string ServiceName { get; init; }

    /// <summary>
    /// User applying the tags.
    /// </summary>
    public string? AppliedBy { get; init; }
}

/// <summary>
/// Response from tag assignment operations.
/// </summary>
public record TagAssignmentResponse
{
    /// <summary>
    /// Whether operation succeeded.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Message describing result.
    /// </summary>
    public required string Message { get; init; }

    /// <summary>
    /// Resource ID that was tagged.
    /// </summary>
    public required Guid ResourceId { get; init; }

    /// <summary>
    /// Applied tag IDs.
    /// </summary>
    public required IEnumerable<Guid> AppliedTagIds { get; init; } = Enumerable.Empty<Guid>();

    /// <summary>
    /// Total tags now on resource.
    /// </summary>
    public required int TotalTagsOnResource { get; init; }

    /// <summary>
    /// Errors encountered, if any.
    /// </summary>
    public IEnumerable<string> Errors { get; init; } = Enumerable.Empty<string>();
}

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

/// <summary>
/// Request for tag suggestions (auto-complete, recommendations).
/// </summary>
public record TagSuggestionRequest
{
    /// <summary>
    /// Search query (partial tag name).
    /// </summary>
    public required string Query { get; init; }

    /// <summary>
    /// Resource type context.
    /// </summary>
    public string? ResourceType { get; init; }

    /// <summary>
    /// Service context.
    /// </summary>
    public string? ServiceName { get; init; }

    /// <summary>
    /// Category filter.
    /// </summary>
    public string? Category { get; init; }

    /// <summary>
    /// Maximum suggestions to return.
    /// </summary>
    public int Limit { get; init; } = 10;
}

/// <summary>
/// Response with tag suggestions.
/// </summary>
public record TagSuggestionResponse
{
    /// <summary>
    /// Suggested tags.
    /// </summary>
    public required IEnumerable<TagDto> Suggestions { get; init; }

    /// <summary>
    /// Total matching tags (may exceed suggestions count if limited).
    /// </summary>
    public required int TotalMatches { get; init; }
}

/// <summary>
/// Bulk tag operation request.
/// </summary>
public record BulkTagOperationRequest
{
    /// <summary>
    /// Operation type: "apply", "remove", "set"
    /// </summary>
    public required string Operation { get; init; }

    /// <summary>
    /// Resource IDs to operate on.
    /// </summary>
    public required IEnumerable<Guid> ResourceIds { get; init; }

    /// <summary>
    /// Resource type.
    /// </summary>
    public required string ResourceType { get; init; }

    /// <summary>
    /// Tag IDs to apply/remove.
    /// </summary>
    public required IEnumerable<Guid> TagIds { get; init; }

    /// <summary>
    /// Service name.
    /// </summary>
    public required string ServiceName { get; init; }

    /// <summary>
    /// User performing operation.
    /// </summary>
    public string? AppliedBy { get; init; }
}

/// <summary>
/// Result of bulk tag operation.
/// </summary>
public record BulkTagOperationResult
{
    /// <summary>
    /// Total resources processed.
    /// </summary>
    public required int TotalProcessed { get; init; }

    /// <summary>
    /// Successfully updated count.
    /// </summary>
    public required int SuccessCount { get; init; }

    /// <summary>
    /// Failed count.
    /// </summary>
    public required int FailureCount { get; init; }

    /// <summary>
    /// Detailed results per resource.
    /// </summary>
    public required IEnumerable<BulkOperationItemResult> Results { get; init; }
}

/// <summary>
/// Result for single item in bulk operation.
/// </summary>
public record BulkOperationItemResult
{
    /// <summary>
    /// Resource ID.
    /// </summary>
    public required Guid ResourceId { get; init; }

    /// <summary>
    /// Whether operation succeeded for this item.
    /// </summary>
    public required bool Success { get; init; }

    /// <summary>
    /// Result message.
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// Error, if any.
    /// </summary>
    public string? Error { get; init; }
}
