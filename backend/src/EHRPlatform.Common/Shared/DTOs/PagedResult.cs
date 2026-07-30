#nullable enable

namespace EHRPlatform.Common.Shared.DTOs;

/// <summary>
/// Generic paginated result container for any entity type.
/// Use this instead of service-specific ListDto classes.
/// </summary>
public class PagedResult<T> where T : class
{
    /// <summary>
    /// The page of items.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Total count of items matching the query (across all pages).
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// Current page number (1-indexed).
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages.
    /// </summary>
    public int TotalPages => (Total + PageSize - 1) / PageSize;

    /// <summary>
    /// Whether there are more pages after this one.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Whether there are pages before this one.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    /// <summary>
    /// Create a paged result from a query, count, and page info.
    /// </summary>
    public static PagedResult<T> Create(List<T> items, int total, int pageNumber, int pageSize)
    {
        return new PagedResult<T>
        {
            Items = items,
            Total = total,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}

