#nullable enable

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.Common.Application.Features.Search.Services;

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
