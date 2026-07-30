#nullable enable

namespace EHRPlatform.Common.Shared.DTOs;

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
