using System;
using System.Collections.Generic;

namespace EHRPlatform.Contracts.Responses;

/// <summary>
/// Paginated API response for list endpoints.
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
    /// Total number of pages.
    /// </summary>
    public int TotalPages => (TotalCount + PageSize - 1) / PageSize;

    /// <summary>
    /// Whether there are more pages.
    /// </summary>
    public bool HasNextPage => PageNumber < TotalPages;

    /// <summary>
    /// Whether there are previous pages.
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
    /// Create from query results.
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

/// <summary>
/// Paginated API response wrapped in ApiResponse envelope.
/// </summary>
public class ApiResponse<T> where T : class
{
    public static Contracts.Responses.ApiResponse<PaginatedResponse<T>> Ok(
        List<T> items,
        int totalCount,
        int pageNumber = 1,
        int pageSize = 10,
        string? message = "Request successful",
        string? traceId = null)
    {
        var paginatedData = PaginatedResponse<T>.Create(items, totalCount, pageNumber, pageSize);
        return new Contracts.Responses.ApiResponse<PaginatedResponse<T>>(true, 200, paginatedData, message)
        {
            TraceId = traceId
        };
    }
}
