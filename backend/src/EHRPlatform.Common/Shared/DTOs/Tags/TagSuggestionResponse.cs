#nullable enable

namespace EHRPlatform.Common.Shared.DTOs;

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
