#nullable enable

namespace EHRPlatform.Common.Shared.DTOs;

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
