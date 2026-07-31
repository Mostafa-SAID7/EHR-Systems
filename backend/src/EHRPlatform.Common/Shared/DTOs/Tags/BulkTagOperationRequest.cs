#nullable enable

namespace EHRPlatform.Common.Shared.DTOs;

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
