using System.Collections.Generic;

namespace EHRPlatform.Common.Search;

/// <summary>
/// Search result with aggregations (facets).
/// Single responsibility: Search results with faceted data.
/// </summary>
public class SearchResultWithAggregations<T> where T : class
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

    /// <summary>
    /// Aggregations/facets.
    /// </summary>
    public Dictionary<string, AggregationBucket[]> Aggregations { get; set; } = new();
}

/// <summary>
/// Aggregation bucket (facet value with count).
/// Single responsibility: Facet bucket data.
/// </summary>
public class AggregationBucket
{
    /// <summary>
    /// Bucket key/value.
    /// </summary>
    public string Key { get; set; } = null!;

    /// <summary>
    /// Document count in bucket.
    /// </summary>
    public long Count { get; set; }
}
