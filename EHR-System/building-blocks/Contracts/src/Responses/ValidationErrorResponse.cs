using System.Collections.Generic;

namespace EHRPlatform.Contracts.Responses;

/// <summary>
/// Validation error response.
/// Single responsibility: Validation error information.
/// </summary>
public class ValidationErrorResponse
{
    /// <summary>
    /// HTTP status code (typically 400).
    /// </summary>
    public int StatusCode { get; set; } = 400;

    /// <summary>
    /// Error message.
    /// </summary>
    public string Message { get; set; } = "Validation failed";

    /// <summary>
    /// Field errors (field name -> error messages).
    /// </summary>
    public Dictionary<string, string[]> Errors { get; set; } = new();

    /// <summary>
    /// Request ID for tracing.
    /// </summary>
    public string? TraceId { get; set; }

    /// <summary>
    /// Timestamp when error occurred.
    /// </summary>
    public System.DateTime Timestamp { get; set; } = System.DateTime.UtcNow;
}
