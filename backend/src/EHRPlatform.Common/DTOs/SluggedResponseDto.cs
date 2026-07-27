#nullable enable

namespace EHRPlatform.Common.DTOs;

/// <summary>
/// Base response DTO that includes slug-friendly URL representation.
/// Inherit from this to provide consistent slug support across all response models.
/// </summary>
public abstract class SluggedResponseDto
{
    /// <summary>
    /// Unique identifier (GUID).
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// URL-friendly slug representation of the entity.
    /// For backward compatibility, both GUID and slug URLs should be supported.
    /// </summary>
    public string? Slug { get; set; }

    /// <summary>
    /// Display name for the slug (readable version).
    /// Useful for UI breadcrumbs and navigation.
    /// </summary>
    public string? SlugDisplayName { get; set; }
}

/// <summary>
/// Base response DTO with status tracking and slug support.
/// Use for entities that have a Status field.
/// </summary>
public abstract class StatusDto : SluggedResponseDto
{
    /// <summary>
    /// Current status as string (e.g., "Active", "Draft", "Completed").
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// URL-friendly slug representation of the status.
    /// </summary>
    public string? StatusSlug { get; set; }
}

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
