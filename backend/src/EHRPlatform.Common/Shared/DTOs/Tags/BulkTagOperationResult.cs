#nullable enable

namespace EHRPlatform.Common.Shared.DTOs;

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
