#nullable enable

using EHRPlatform.Common.Domain.Constants;

namespace EHRPlatform.Common.Domain.Exceptions;

/// <summary>
/// Exception thrown when an operation times out.
/// Single responsibility: Represent timeout failures only.
/// </summary>
public class TimeoutException : DomainException
{
    /// <summary>
    /// Error code for timeout errors.
    /// </summary>
    public override string ErrorCode => ErrorCode.TimeoutError;

    /// <summary>
    /// Initialize with timeout error message.
    /// </summary>
    public TimeoutException(string message)
        : base(message, ErrorCode.TimeoutError)
    {
    }
}
