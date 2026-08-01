using System.Collections.Generic;

namespace EHRPlatform.Contracts.Responses;

/// <summary>
/// Error details included in error responses.
/// Single responsibility: Error detail structure.
/// </summary>
public class ErrorDetails
{
    /// <summary>
    /// Error message.
    /// </summary>
    public string Message { get; set; } = null!;

    /// <summary>
    /// Detailed error information.
    /// </summary>
    public List<string> Details { get; set; } = new();

    /// <summary>
    /// Error code for classification.
    /// </summary>
    public string? ErrorCode { get; set; }

    /// <summary>
    /// Exception type (development only).
    /// </summary>
    public string? ExceptionType { get; set; }
}
