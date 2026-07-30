#nullable enable

namespace EHRPlatform.Common.Shared.DTOs;

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
