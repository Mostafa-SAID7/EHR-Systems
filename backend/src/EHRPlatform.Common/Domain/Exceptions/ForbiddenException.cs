#nullable enable

using EHRPlatform.Common.Domain.Constants;

namespace EHRPlatform.Common.Domain.Exceptions;

/// <summary>
/// Exception thrown when user attempts an action without authorization.
/// Single responsibility: Represent forbidden/authorization failures only.
/// </summary>
public class ForbiddenException : DomainException
{
    /// <summary>
    /// Error code for forbidden access.
    /// </summary>
    public override string ErrorCode => ErrorCode.Forbidden;

    /// <summary>
    /// Initialize with forbidden action message.
    /// </summary>
    public ForbiddenException(string message)
        : base(message, ErrorCode.Forbidden)
    {
    }
}
