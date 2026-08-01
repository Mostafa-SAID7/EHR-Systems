using System.Collections.Generic;

namespace EHRPlatform.Contracts.Responses;

/// <summary>
/// Typed API response with data payload.
/// Single responsibility: Success response with typed data.
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
