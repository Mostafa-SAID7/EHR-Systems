#nullable enable

namespace EHRPlatform.Common.Shared.Responses;

/// <summary>
/// RFC 7807 Problem Details response format.
/// Standardized error response used across all services.
/// </summary>
public sealed class ProblemDetails
{
    /// <summary>
    /// HTTP status code.
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// Error title/category.
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Error code for client-side error handling.
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// Detailed error message (null in production for security/HIPAA).
    /// </summary>
    public string? Detail { get; set; }

    /// <summary>
    /// Correlation ID for linking requests to logs.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Additional validation errors organized by field.
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// RFC 7807 standard type URI (optional).
    /// </summary>
    public string? Type { get; set; }

    /// <summary>
    /// RFC 7807 standard instance URI (optional).
    /// </summary>
    public string? Instance { get; set; }

    /// <summary>
    /// Create a simple problem details response.
    /// </summary>
    public static ProblemDetails Create(int status, string title, string errorCode, string correlationId, string? detail = null)
    {
        return new ProblemDetails
        {
            Status = status,
            Title = title,
            ErrorCode = errorCode,
            CorrelationId = correlationId,
            Detail = detail
        };
    }

    /// <summary>
    /// Create a validation error response with field errors.
    /// </summary>
    public static ProblemDetails CreateValidation(string correlationId, Dictionary<string, string[]> errors)
    {
        return new ProblemDetails
        {
            Status = 422,
            Title = "Validation Error",
            ErrorCode = "VALIDATION_ERROR",
            CorrelationId = correlationId,
            Errors = errors,
            Type = "https://example.com/errors/validation"
        };
    }

    /// <summary>
    /// Create a not found error response.
    /// </summary>
    public static ProblemDetails CreateNotFound(string resourceType, string correlationId)
    {
        return new ProblemDetails
        {
            Status = 404,
            Title = "Resource Not Found",
            ErrorCode = "NOT_FOUND",
            CorrelationId = correlationId,
            Detail = $"The requested {resourceType} was not found."
        };
    }

    /// <summary>
    /// Create an unauthorized error response.
    /// </summary>
    public static ProblemDetails CreateUnauthorized(string correlationId)
    {
        return new ProblemDetails
        {
            Status = 401,
            Title = "Unauthorized",
            ErrorCode = "UNAUTHORIZED",
            CorrelationId = correlationId,
            Detail = "Authentication required."
        };
    }

    /// <summary>
    /// Create a forbidden error response.
    /// </summary>
    public static ProblemDetails CreateForbidden(string correlationId)
    {
        return new ProblemDetails
        {
            Status = 403,
            Title = "Forbidden",
            ErrorCode = "FORBIDDEN",
            CorrelationId = correlationId,
            Detail = "You do not have permission to access this resource."
        };
    }

    /// <summary>
    /// Create a conflict error response.
    /// </summary>
    public static ProblemDetails CreateConflict(string detail, string correlationId)
    {
        return new ProblemDetails
        {
            Status = 409,
            Title = "Conflict",
            ErrorCode = "CONFLICT",
            CorrelationId = correlationId,
            Detail = detail
        };
    }

    /// <summary>
    /// Create an internal server error response.
    /// </summary>
    public static ProblemDetails CreateInternalError(string correlationId, string? detail = null)
    {
        return new ProblemDetails
        {
            Status = 500,
            Title = "Internal Server Error",
            ErrorCode = "INTERNAL_ERROR",
            CorrelationId = correlationId,
            Detail = detail
        };
    }
}

