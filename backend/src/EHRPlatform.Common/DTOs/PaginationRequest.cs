#nullable enable

namespace EHRPlatform.Common.DTOs;

/// <summary>
/// Standard pagination request parameters.
/// Use this instead of service-specific pagination DTOs.
/// </summary>
public class PaginationRequest
{
    /// <summary>
    /// Page number (1-indexed).
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// Number of items per page.
    /// </summary>
    public int PageSize { get; set; } = 10;

    /// <summary>
    /// Optional search/filter query.
    /// </summary>
    public string? SearchQuery { get; set; }

    /// <summary>
    /// Optional sort field name.
    /// </summary>
    public string? SortBy { get; set; }

    /// <summary>
    /// Sort order: "asc" or "desc".
    /// </summary>
    public string? SortOrder { get; set; } = "asc";

    /// <summary>
    /// Validate pagination parameters.
    /// </summary>
    public bool IsValid()
    {
        return PageNumber >= 1 && PageSize >= 1 && PageSize <= 100;
    }

    /// <summary>
    /// Get the number of records to skip.
    /// </summary>
    public int GetSkip() => (PageNumber - 1) * PageSize;
}
