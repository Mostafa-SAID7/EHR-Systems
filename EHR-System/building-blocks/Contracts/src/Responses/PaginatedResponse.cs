using System.Collections.Generic;

namespace EHRPlatform.Contracts.Responses;

/// <summary>
/// Paginated response containing list of items with pagination metadata.
/// Single responsibility: Pagination data transfer.
/// </summary>
public class PaginatedResponse<T>
{
    /// <summary>
    /// List of items in current page.
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// Total count of items (not just page count).
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// Current page number (1-indexed).
    /// </summary>
    public int PageNumber { get; set; }

    /// <summary>
    /// Items per page.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Total number of pages (calculated).
    /// </summary>
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

    /// <summary>
    /// Whether there are more pages after current.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Whether there are pages before current.
    /// </summary>
    public bool HasPreviousPage => PageNumber > 1;

    public PaginatedResponse()
    {
    }

    public PaginatedResponse(List<T> items, int totalCount, int pageNumber, int pageSize)
    {
        Items = items;
        TotalCount = totalCount;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }

    /// <summary>
    /// Factory method to create paginated response from query results.
    /// </summary>
    public static PaginatedResponse<T> Create(
        List<T> items,
        int totalCount,
        int pageNumber = 1,
        int pageSize = 10)
    {
        return new PaginatedResponse<T>(items, totalCount, pageNumber, pageSize);
    }
}
