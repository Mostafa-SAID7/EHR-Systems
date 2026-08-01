using System;
using System.Collections.Generic;

namespace EHRPlatform.Contracts.Responses;

/// <summary>
/// Standard API response envelope for all endpoints.
/// Single responsibility: Success/failure response with metadata.
/// </summary>
public class ApiResponse
{
    /// <summary>
    /// Whether the request succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// HTTP status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Human-readable message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Error details (null if successful).
    /// </summary>
    public ErrorDetails? ErrorInfo { get; set; }

    /// <summary>
    /// Request trace ID for debugging and logging.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Timestamp of response.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ApiResponse()
    {
    }

    public ApiResponse(bool success, int statusCode, string? message = null)
    {
        Success = success;
        StatusCode = statusCode;
        Message = message;
    }

    /// <summary>
    /// Create successful response.
    /// </summary>
    public static ApiResponse Ok(string? message = "Request successful", string? traceId = null)
    {
        return new ApiResponse(true, 200, message) { TraceId = traceId };
    }

    /// <summary>
    /// Create error response.
    /// </summary>
    public static ApiResponse Failure(int statusCode, string message, List<string>? details = null, string? traceId = null)
    {
        return new ApiResponse(false, statusCode, message)
        {
            ErrorInfo = new ErrorDetails { Message = message, Details = details ?? new List<string>() },
            TraceId = traceId
        };
    }

    /// <summary>
    /// Create not found response.
    /// </summary>
    public static ApiResponse NotFound(string message = "Resource not found", string? traceId = null)
    {
        return Failure(404, message, traceId: traceId);
    }

    /// <summary>
    /// Create validation error response.
    /// </summary>
    public static ApiResponse ValidationFailed(List<string> errors, string? traceId = null)
    {
        return Failure(400, "Validation failed", errors, traceId);
    }

    /// <summary>
    /// Create unauthorized response.
    /// </summary>
    public static ApiResponse Unauthorized(string message = "Unauthorized", string? traceId = null)
    {
        return Failure(401, message, traceId: traceId);
    }

    /// <summary>
    /// Create forbidden response.
    /// </summary>
    public static ApiResponse Forbidden(string message = "Forbidden", string? traceId = null)
    {
        return Failure(403, message, traceId: traceId);
    }

    /// <summary>
    /// Create server error response.
    /// </summary>
    public static ApiResponse InternalServerError(string message = "An error occurred", string? traceId = null)
    {
        return Failure(500, message, traceId: traceId);
    }
}

/// <summary>
/// Typed API response with data payload.
/// Single responsibility: Success response with data.
/// </summary>
public class ApiResponse<T> : ApiResponse
{
    /// <summary>
    /// Response data.
    /// </summary>
    public T? Data { get; set; }

    public ApiResponse()
    {
    }

    public ApiResponse(bool success, int statusCode, T? data = default, string? message = null)
        : base(success, statusCode, message)
    {
        Data = data;
    }

    /// <summary>
    /// Create successful response with data.
    /// </summary>
    public static ApiResponse<T> Ok(T? data, string? message = "Request successful", string? traceId = null)
    {
        return new ApiResponse<T>(true, 200, data, message) { TraceId = traceId };
    }

    /// <summary>
    /// Create error response.
    /// </summary>
    public static new ApiResponse<T> Failure(int statusCode, string message, List<string>? details = null, string? traceId = null)
    {
        return new ApiResponse<T>(false, statusCode, default, message)
        {
            ErrorInfo = new ErrorDetails { Message = message, Details = details ?? new List<string>() },
            TraceId = traceId
        };
    }

    /// <summary>
    /// Create not found response.
    /// </summary>
    public static ApiResponse<T> NotFound(string message = "Resource not found", string? traceId = null)
    {
        return Failure(404, message, traceId: traceId);
    }

    /// <summary>
    /// Create created response (201).
    /// </summary>
    public static ApiResponse<T> Created(T data, string? message = "Resource created", string? traceId = null)
    {
        return new ApiResponse<T>(true, 201, data, message) { TraceId = traceId };
    }
}
