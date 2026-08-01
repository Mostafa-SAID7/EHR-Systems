namespace EHRPlatform.Common.Search;

/// <summary>
/// Sort clause for search results.
/// Single responsibility: Sort specification.
/// </summary>
public class SortClause
{
    /// <summary>
    /// Field name to sort by.
    /// </summary>
    public string Field { get; set; } = null!;

    /// <summary>
    /// Sort direction (asc or desc).
    /// </summary>
    public string Direction { get; set; } = "asc";
}
