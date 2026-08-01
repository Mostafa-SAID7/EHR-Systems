namespace EHRPlatform.Common.Search;

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
