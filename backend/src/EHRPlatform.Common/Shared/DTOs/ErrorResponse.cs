#nullable enable

namespace EHRPlatform.Common.Shared.DTOs;

/// <summary>
/// Standard error response for all services (RFC 7807 Problem Details).
/// </summary>
public class ErrorResponse
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
    /// Error code for client-side handling.
    /// </summary>
    public string ErrorCode { get; set; } = string.Empty;

    /// <summary>
    /// Detailed error message (null in production for security).
    /// </summary>
    public string? Detail { get; set; }

    /// <summary>
    /// Correlation ID for tracking related requests.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Additional error details (validation errors, etc).
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// Create a simple error response.
    /// </summary>
    public static ErrorResponse Create(int status, string title, string errorCode, string correlationId)
    {
        return new ErrorResponse
        {
            Status = status,
            Title = title,
            ErrorCode = errorCode,
            CorrelationId = correlationId
        };
    }

    /// <summary>
    /// Create an error response with validation errors.
    /// </summary>
    public static ErrorResponse CreateValidation(string correlationId, Dictionary<string, string[]> errors)
    {
        return new ErrorResponse
        {
            Status = 422,
            Title = "Validation Error",
            ErrorCode = "VALIDATION_ERROR",
            CorrelationId = correlationId,
            Errors = errors
        };
    }
}

