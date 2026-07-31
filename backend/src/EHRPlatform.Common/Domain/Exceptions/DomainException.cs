#nullable enable

namespace EHRPlatform.Common.Domain.Exceptions;

/// <summary>
/// Base exception for domain-related errors.
/// All domain exceptions inherit from this to provide consistent error handling.
/// Single responsibility: Define domain exception contract only.
/// </summary>
public abstract class DomainException : Exception
{
    /// <summary>
    /// Gets the error code for the exception.
    /// Used for error categorization and client-side handling.
    /// </summary>
    public virtual string ErrorCode { get; }

    /// <summary>
    /// Gets the correlation ID for tracking across systems.
    /// </summary>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Initialize a domain exception with message and error code.
    /// </summary>
    protected DomainException(string message, string errorCode, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
    }
}
