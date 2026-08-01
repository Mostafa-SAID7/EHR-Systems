using System.Collections.Generic;

namespace EHRPlatform.Contracts.Responses;

/// <summary>
/// Generic typed API response.
/// Single responsibility: Generic typed response envelope.
/// </summary>
public class ApiResponse<T>
{
    /// <summary>
    /// Success flag.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Response message.
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// Response data.
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// Error details (if failed).
    /// </summary>
    public ErrorDetails? Error { get; set; }

    /// <summary>
    /// Errors list (for validation).
    /// </summary>
    public Dictionary<string, string[]>? Errors { get; set; }

    /// <summary>
    /// Request ID for tracing.
    /// </summary>
    public string? TraceId { get; set; }
}
