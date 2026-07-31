#nullable enable

using EHRPlatform.Common.Domain.Constants;

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
            Status = HttpStatusMap.UnprocessableEntity,
            Title = "Validation Error",
            ErrorCode = ErrorCode.ValidationError,
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
            Status = HttpStatusMap.NotFound,
            Title = "Resource Not Found",
            ErrorCode = ErrorCode.NotFound,
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
            Status = HttpStatusMap.Unauthorized,
            Title = "Unauthorized",
            ErrorCode = ErrorCode.Unauthorized,
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
            Status = HttpStatusMap.Forbidden,
            Title = "Forbidden",
            ErrorCode = ErrorCode.Forbidden,
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
            Status = HttpStatusMap.Conflict,
            Title = "Conflict",
            ErrorCode = ErrorCode.Conflict,
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
            Status = HttpStatusMap.InternalServerError,
            Title = "Internal Server Error",
            ErrorCode = ErrorCode.InternalError,
            CorrelationId = correlationId,
            Detail = detail
        };
    }
}

