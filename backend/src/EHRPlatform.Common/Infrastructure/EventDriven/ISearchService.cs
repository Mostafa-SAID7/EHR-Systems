#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.Common.Search;

/// <summary>
/// Search service interface for Elasticsearch integration.
/// Provides full-text search, indexing, and querying capabilities.
/// Supports medical terminology and clinical data search.
/// </summary>
public interface ISearchService
{
    /// <summary>
    /// Search for entities by query.
    /// Supports full-text search, filters, and facets.
    /// </summary>
    Task<SearchResult<T>> SearchAsync<T>(
        SearchQuery query,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Index single entity.
    /// Creates or updates in Elasticsearch index.
    /// </summary>
    Task IndexAsync<T>(
        string id,
        T entity,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Index multiple entities in bulk.
    /// More efficient than individual indexes.
    /// </summary>
    Task IndexBulkAsync<T>(
        IEnumerable<(string id, T entity)> items,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Delete entity from index.
    /// </summary>
    Task DeleteAsync<T>(
        string id,
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Rebuild entire index for entity type.
    /// Used for migrations and schema changes.
    /// </summary>
    Task RebuildIndexAsync<T>(
        CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Check if index exists.
    /// </summary>
    Task<bool> IndexExistsAsync<T>(CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Delete entire index.
    /// WARNING: Destructive operation.
    /// </summary>
    Task DeleteIndexAsync<T>(CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Get search statistics.
    /// </summary>
    Task<SearchStatistics> GetStatisticsAsync(CancellationToken cancellationToken = default);
}

/// <summary>
/// Search query for Elasticsearch.
/// Supports full-text search, filters, sorting, and pagination.
/// </summary>
public class SearchQuery
{
    /// <summary>
    /// Query text for full-text search.
    /// Example: "diabetes patient" searches across all indexed fields.
    /// </summary>
    public string? QueryText { get; set; }

    /// <summary>
    /// Field-specific search.
    /// Example: ("FirstName", "John") searches only FirstName field.
    /// </summary>
    public Dictionary<string, string>? FieldFilters { get; set; }

    /// <summary>
    /// Date range filter.
    /// </summary>
    public (DateTime? From, DateTime? To)? DateRange { get; set; }

    /// <summary>
    /// Pagination - page number (1-based).
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Pagination - items per page (max 100).
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Fields to sort by.
    /// Example: ("CreatedAt", SortOrder.Descending)
    /// </summary>
    public List<(string field, SortOrder order)>? SortBy { get; set; }

    /// <summary>
    /// Highlight search results in response.
    /// </summary>
    public bool HighlightResults { get; set; } = true;

    /// <summary>
    /// Request facets (aggregations).
    /// Example: ["Status", "Department"] - returns counts per value.
    /// </summary>
    public List<string>? Facets { get; set; }
}

/// <summary>
/// Search result wrapper with pagination and facets.
/// </summary>
public class SearchResult<T> where T : class
{
    public List<SearchHit<T>> Hits { get; set; } = new();
    public long TotalCount { get; set; }
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public Dictionary<string, Dictionary<string, long>>? Facets { get; set; }
}

/// <summary>
/// Single search result with score and highlights.
/// </summary>
public class SearchHit<T> where T : class
{
    public string Id { get; set; } = string.Empty;
    public T Document { get; set; } = default!;
    public double Score { get; set; }
    public Dictionary<string, string[]>? Highlights { get; set; }
}

/// <summary>
/// Sort order for search results.
/// </summary>
public enum SortOrder
{
    Ascending,
    Descending
}

/// <summary>
/// Search statistics for monitoring.
/// </summary>
public class SearchStatistics
{
    public Dictionary<string, long> IndexDocumentCounts { get; set; } = new();
    public Dictionary<string, long> IndexSizeBytes { get; set; } = new();
    public long TotalDocuments { get; set; }
    public long TotalSizeBytes { get; set; }
    public DateTime CollectedAt { get; set; } = DateTime.UtcNow;
}
