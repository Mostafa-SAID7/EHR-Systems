namespace EHRPlatform.Common.Sorting;

/// <summary>
/// Interface for consistent sorting across API endpoints.
/// Single responsibility: Sorting specification contract.
/// </summary>
public interface ISortingProvider
{
    /// <summary>
    /// Add sort clause.
    /// </summary>
    ISortingProvider AddSort(string fieldName, SortDirection direction);

    /// <summary>
    /// Get sort specifications.
    /// </summary>
    IReadOnlyList<SortSpecification> GetSpecifications();

    /// <summary>
    /// Clear all sorts.
    /// </summary>
    void Clear();

    /// <summary>
    /// Get sort string for API response.
    /// </summary>
    string GetSortString();
}
