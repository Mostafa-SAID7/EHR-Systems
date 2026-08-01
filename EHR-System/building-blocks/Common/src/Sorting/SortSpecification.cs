namespace EHRPlatform.Common.Sorting;

/// <summary>
/// Sort specification for a single field.
/// Single responsibility: Sort specification data structure.
/// </summary>
public class SortSpecification
{
    /// <summary>
    /// Field name to sort by.
    /// </summary>
    public string FieldName { get; set; } = null!;

    /// <summary>
    /// Sort direction.
    /// </summary>
    public SortDirection Direction { get; set; }

    /// <summary>
    /// Sort priority (lower = higher priority).
    /// </summary>
    public int Priority { get; set; }
}
