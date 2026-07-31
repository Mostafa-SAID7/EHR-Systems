#nullable enable

using EHRPlatform.Common.Domain.Constants;

namespace EHRPlatform.Common.Domain.Exceptions;

/// <summary>
/// Exception thrown when a resource already exists (conflict).
/// Single responsibility: Represent resource conflicts only.
/// </summary>
public class ConflictException : DomainException
{
    /// <summary>
    /// Error code for conflict errors.
    /// </summary>
    public override string ErrorCode => ErrorCode.Conflict;

    /// <summary>
    /// Initialize with conflict error message.
    /// </summary>
    public ConflictException(string message)
        : base(message, ErrorCode.Conflict)
    {
    }
}
