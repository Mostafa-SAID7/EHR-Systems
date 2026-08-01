using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.Common.Search;

/// <summary>
/// Interface for full-text search service.
/// Single responsibility: Execute search queries against indexed content.
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Execute full-text search.
    /// </summary>
    Task<SearchResult<T>> SearchAsync<T>(SearchQuery query, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Search with aggregations.
    /// </summary>
    Task<SearchResultWithAggregations<T>> SearchWithAggregationsAsync<T>(SearchQuery query, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Get search suggestions/autocomplete.
    /// </summary>
    Task<IReadOnlyList<string>> GetSuggestionsAsync(string prefix, string field, int maxResults = 10, CancellationToken cancellationToken = default);

    /// <summary>
    /// Search with highlighting.
    /// </summary>
    Task<IReadOnlyList<SearchHit<T>>> SearchWithHighlightAsync<T>(SearchQuery query, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Count documents matching query.
    /// </summary>
    Task<long> CountAsync(SearchQuery query, CancellationToken cancellationToken = default);
}

/// <summary>
/// Search query builder.
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

/// <summary>
/// Search result.
/// Single responsibility: Search result data.
/// </summary>
public class SearchResult<T> where T : class
{
    /// <summary>
    /// Total matches.
    /// </summary>
    public long Total { get; set; }

    /// <summary>
    /// Search duration in milliseconds.
    /// </summary>
    public long DurationMs { get; set; }

    /// <summary>
    /// Result items.
    /// </summary>
    public IReadOnlyList<T> Items { get; set; } = new List<T>();
}
