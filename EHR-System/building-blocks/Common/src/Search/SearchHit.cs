using System.Collections.Generic;

namespace EHRPlatform.Common.Search;

/// <summary>
/// Search hit with highlights.
/// Single responsibility: Search result with highlighting.
/// </summary>
public class SearchHit<T> where T : class
{
    /// <summary>
    /// Document ID.
    /// </summary>
    public string Id { get; set; } = null!;

    /// <summary>
    /// Document content.
    /// </summary>
    public T Document { get; set; } = null!;

    /// <summary>
    /// Search score/relevance.
    /// </summary>
    public double Score { get; set; }

    /// <summary>
    /// Highlighted snippets by field.
    /// </summary>
    public Dictionary<string, List<string>> Highlights { get; set; } = new();
}
