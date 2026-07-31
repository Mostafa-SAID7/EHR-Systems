#nullable enable

namespace EHRPlatform.Common.Domain.Enums;

/// <summary>
/// Sort order for search and query results.
/// </summary>
public enum SortOrder
{
    /// <summary>Sort in ascending order (A-Z, oldest first).</summary>
    Ascending = 0,

    /// <summary>Sort in descending order (Z-A, newest first).</summary>
    Descending = 1
}
