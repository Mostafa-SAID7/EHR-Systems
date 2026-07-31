#nullable enable

namespace EHRPlatform.Common.Shared.DTOs;

/// <summary>
/// Paginated tag search results.
/// </summary>
public record TagSearchResult
{
    /// <summary>List of resource IDs matching search criteria.</summary>
    public required IEnumerable<Guid> ResourceIds { get; init; }

    /// <summary>Total count of matching resources.</summary>
    public required int TotalCount { get; init; }

    /// <summary>Current page number.</summary>
    public required int PageNumber { get; init; }

    /// <summary>Page size.</summary>
    public required int PageSize { get; init; }

    /// <summary>Total pages available.</summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
}
