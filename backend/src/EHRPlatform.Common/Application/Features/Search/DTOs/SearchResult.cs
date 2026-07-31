#nullable enable

namespace EHRPlatform.Common.Application.Features.Search.Services;

/// <summary>
/// Search result wrapper with pagination and facets.
/// </summary>
public class SearchResult<T> where T : class
{
    /// <summary>
    /// List of search hits/matches.
    /// </summary>
    public List<SearchHit<T>> Hits { get; set; } = new();

    /// <summary>
    /// Total number of matching documents.
    /// </summary>
    public long TotalCount { get; set; }

    /// <summary>
    /// Current page number.
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Facet aggregations (field value counts).
    /// </summary>
    public Dictionary<string, Dictionary<string, long>>? Facets { get; set; }
}

/// <summary>
/// Single search result with score and highlights.
/// </summary>
public class SearchHit<T> where T : class
{
    /// <summary>
    /// Document ID from index.
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// The matching document.
    /// </summary>
    public T Document { get; set; } = default!;

    /// <summary>
    /// Relevance score (higher = more relevant).
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// Highlighted snippets of matching text.
    /// </summary>
    public Dictionary<string, string[]>? Highlights { get; set; }
}
