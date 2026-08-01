using System;

namespace EHRPlatform.Common.Exceptions;

/// <summary>
/// Base exception for application-specific errors.
/// Single responsibility: Application exception base class.
/// </summary>
public class ApplicationException : Exception
{
    /// <summary>
    /// Error code for categorization.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Whether error is user-facing.
    /// </summary>
    public bool IsUserFacingError { get; }

    /// <summary>
    /// Additional error details.
    /// </summary>
    public Dictionary<string, object>? Details { get; }

    public ApplicationException(
        string message,
        string? errorCode = null,
        bool isUserFacingError = false,
        Dictionary<string, object>? details = null,
        Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode ?? "APP_ERROR";
        IsUserFacingError = isUserFacingError;
        Details = details;
    }
}
