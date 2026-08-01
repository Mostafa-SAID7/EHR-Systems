using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.Common.Search;

/// <summary>
/// Interface for managing search indices.
/// Single responsibility: Create and manage search indices.
/// </summary>
public interface IIndexService
{
    /// <summary>
    /// Create index for type.
    /// </summary>
    Task CreateIndexAsync<T>(string indexName, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Delete index.
    /// </summary>
    Task DeleteIndexAsync(string indexName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Index document.
    /// </summary>
    Task IndexAsync<T>(string indexName, string documentId, T document, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Batch index documents.
    /// </summary>
    Task BulkIndexAsync<T>(string indexName, IEnumerable<(string Id, T Document)> documents, CancellationToken cancellationToken = default) where T : class;

    /// <summary>
    /// Delete document from index.
    /// </summary>
    Task DeleteAsync(string indexName, string documentId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Refresh index.
    /// </summary>
    Task RefreshIndexAsync(string indexName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Re-index all documents.
    /// </summary>
    Task ReindexAsync(string fromIndex, string toIndex, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get index statistics.
    /// </summary>
    Task<IndexStats> GetStatsAsync(string indexName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if index exists.
    /// </summary>
    Task<bool> IndexExistsAsync(string indexName, CancellationToken cancellationToken = default);
}

/// <summary>
/// Index statistics.
/// Single responsibility: Index statistics data.
/// </summary>
public class IndexStats
{
    /// <summary>
    /// Index name.
    /// </summary>
    public string IndexName { get; set; } = null!;

    /// <summary>
    /// Total documents indexed.
    /// </summary>
    public long DocumentCount { get; set; }

    /// <summary>
    /// Index size in bytes.
    /// </summary>
    public long SizeBytes { get; set; }

    /// <summary>
    /// Last update time.
    /// </summary>
    public System.DateTime? LastUpdated { get; set; }
}
