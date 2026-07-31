#nullable enable

namespace EHRPlatform.Common.Application.Features.Search.Services;

/// <summary>
/// Search service statistics.
/// </summary>
public class SearchStatistics
{
    /// <summary>
    /// Total documents indexed.
    /// </summary>
    public long TotalDocuments { get; set; }

    /// <summary>
    /// Index size in bytes.
    /// </summary>
    public long IndexSizeBytes { get; set; }

    /// <summary>
    /// Whether index is healthy.
    /// </summary>
    public bool IsHealthy { get; set; }

    /// <summary>
    /// Number of shards.
    /// </summary>
    public int Shards { get; set; }

    /// <summary>
    /// Number of replicas.
    /// </summary>
    public int Replicas { get; set; }
}
