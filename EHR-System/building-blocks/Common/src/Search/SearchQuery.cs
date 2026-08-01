using System.Collections.Generic;

namespace EHRPlatform.Common.Search;

/// <summary>
/// Search query specification.
/// Single responsibility: Build search queries.
/// </summary>
public class SearchQuery
{
    /// <summary>
    /// Search text.
    /// </summary>
    public string Text { get; set; } = null!;

    /// <summary>
    /// Fields to search in.
    /// </summary>
    public List<string> Fields { get; set; } = new();

    /// <summary>
    /// Filters to apply.
    /// </summary>
    public List<SearchFilter> Filters { get; set; } = new();

    /// <summary>
    /// Sort order.
    /// </summary>
    public List<SortClause> Sort { get; set; } = new();

    /// <summary>
    /// Skip (pagination).
    /// </summary>
    public int Skip { get; set; }

    /// <summary>
    /// Take (pagination).
    /// </summary>
    public int Take { get; set; } = 10;
}
