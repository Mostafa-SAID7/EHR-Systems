namespace EHRPlatform.Common.Search;

/// <summary>
/// Search filter criteria.
/// Single responsibility: Search filter data.
/// </summary>
public class SearchFilter
{
    /// <summary>
    /// Field name to filter on.
    /// </summary>
    public string Field { get; set; } = null!;

    /// <summary>
    /// Operator (equals, gt, lt, contains, etc).
    /// </summary>
    public string Operator { get; set; } = null!;

    /// <summary>
    /// Filter value.
    /// </summary>
    public object Value { get; set; } = null!;
}
