using System.Collections.Generic;

namespace EHRPlatform.Common.Search;

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
