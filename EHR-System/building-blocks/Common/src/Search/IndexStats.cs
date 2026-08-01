using System;

namespace EHRPlatform.Common.Search;

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
    public DateTime? LastUpdated { get; set; }
}
