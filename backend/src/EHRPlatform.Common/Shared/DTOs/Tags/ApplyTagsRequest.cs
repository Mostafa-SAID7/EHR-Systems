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
