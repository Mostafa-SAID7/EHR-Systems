#nullable enable

namespace EHRPlatform.Common.Shared.DTOs;

/// <summary>
/// Paginated response envelope with slug support.
/// Wraps PagedResult with additional metadata for slug-based navigation.
/// </summary>
public class PagedApiResponse<T> where T : class
{
    /// <summary>
    /// Paged items.
    /// </summary>
    public PagedResult<T> Data { get; set; } = new();

    /// <summary>
    /// Slugs for resource collection (if applicable).
    /// Maps item index to slug value for filtering/navigation.
    /// </summary>
    public Dictionary<int, string>? ResourceSlugs { get; set; }

    /// <summary>
    /// Request correlation ID for tracing.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Operation success indicator.
    /// </summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// Error message if operation failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// HTTP status code.
    /// </summary>
    public int StatusCode { get; set; } = 200;

    /// <summary>
    /// Create a success paged response.
    /// </summary>
    public static PagedApiResponse<T> Success(
        PagedResult<T> data,
        Dictionary<int, string>? slugs = null,
        string? correlationId = null)
    {
        return new PagedApiResponse<T>
        {
            Data = data,
            ResourceSlugs = slugs,
            CorrelationId = correlationId,
            IsSuccess = true,
            StatusCode = 200
        };
    }

    /// <summary>
    /// Create a failure paged response.
    /// </summary>
    public static PagedApiResponse<T> Failure(string errorMessage, int statusCode = 400, string? correlationId = null)
    {
        return new PagedApiResponse<T>
        {
            Data = null!,
            CorrelationId = correlationId,
            IsSuccess = false,
            ErrorMessage = errorMessage,
            StatusCode = statusCode
        };
    }
}
