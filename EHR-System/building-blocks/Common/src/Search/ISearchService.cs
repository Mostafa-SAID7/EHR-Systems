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
