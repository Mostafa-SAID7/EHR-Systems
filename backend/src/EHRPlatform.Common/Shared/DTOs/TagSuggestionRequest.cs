#nullable enable

namespace EHRPlatform.Common.Shared.DTOs;

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
