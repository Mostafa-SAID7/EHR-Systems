#nullable enable

namespace EHRPlatform.Common.Shared.DTOs;

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
