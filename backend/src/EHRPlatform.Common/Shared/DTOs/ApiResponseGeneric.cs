#nullable enable

namespace EHRPlatform.Common.Shared.DTOs;

/// <summary>
/// Standard generic success response wrapper for all services.
/// Provides consistent response structure across microservices with typed data.
/// </summary>
public class ApiResponse<T> where T : class
{
    /// <summary>
    /// HTTP status code.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Response message.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The actual response data.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Correlation ID for request tracking.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when response was generated.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Create a successful response.
    /// </summary>
    public static ApiResponse<T> Success(T data, string message = "Success", string correlationId = "")
    {
        return new ApiResponse<T>
        {
            StatusCode = 200,
            Message = message,
            Data = data,
            CorrelationId = correlationId
        };
    }

    /// <summary>
    /// Create a created response (201).
    /// </summary>
    public static ApiResponse<T> Created(T data, string message = "Resource created successfully", string correlationId = "")
    {
        return new ApiResponse<T>
        {
            StatusCode = 201,
            Message = message,
            Data = data,
            CorrelationId = correlationId
        };
    }

    /// <summary>
    /// Create an accepted response (202).
    /// </summary>
    public static ApiResponse<T> Accepted(T data, string message = "Request accepted for processing", string correlationId = "")
    {
        return new ApiResponse<T>
        {
            StatusCode = 202,
            Message = message,
            Data = data,
            CorrelationId = correlationId
        };
    }
}
