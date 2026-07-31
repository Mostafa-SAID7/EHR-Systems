#nullable enable

using EHRPlatform.Common.Domain.Constants;

namespace EHRPlatform.Common.Domain.Exceptions;

/// <summary>
/// Exception thrown when a validation rule is violated.
/// Single responsibility: Represent validation failures only.
/// </summary>
public class ValidationException : DomainException
{
    /// <summary>
    /// Error code for validation failures.
    /// </summary>
    public override string ErrorCode => ErrorCode.ValidationError;

    /// <summary>
    /// Initialize with validation error message.
    /// </summary>
    public ValidationException(string message, Exception? innerException = null)
        : base(message, ErrorCode.ValidationError, innerException)
    {
    }
}
