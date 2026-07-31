#nullable enable

using EHRPlatform.Common.Shared.Utilities.Helpers;

namespace EHRPlatform.Common.Shared.DTOs;

/// <summary>
/// Non-generic API response for operations without return data.
/// </summary>
public class ApiResponse
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
    /// Correlation ID for request tracking.
    /// </summary>
    public string CorrelationId { get; set; } = string.Empty;

    /// <summary>
    /// Timestamp when response was generated.
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTimeHelper.UtcNow;

    /// <summary>
    /// Create a successful response.
    /// </summary>
    public static ApiResponse Success(string message = "Success", string correlationId = "")
    {
        return new ApiResponse
        {
            StatusCode = 200,
            Message = message,
            CorrelationId = correlationId
        };
    }

    /// <summary>
    /// Create an accepted response (202).
    /// </summary>
    public static ApiResponse Accepted(string message = "Request accepted for processing", string correlationId = "")
    {
        return new ApiResponse
        {
            StatusCode = 202,
            Message = message,
            CorrelationId = correlationId
        };
    }

    /// <summary>
    /// Create a no-content response (204).
    /// </summary>
    public static ApiResponse NoContent(string message = "No content", string correlationId = "")
    {
        return new ApiResponse
        {
            StatusCode = 204,
            Message = message,
            CorrelationId = correlationId
        };
    }
}

