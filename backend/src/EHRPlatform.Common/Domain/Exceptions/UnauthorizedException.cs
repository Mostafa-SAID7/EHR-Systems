#nullable enable

using EHRPlatform.Common.Domain.Constants;

namespace EHRPlatform.Common.Domain.Exceptions;

/// <summary>
/// Exception thrown when user lacks required permissions/authentication.
/// Single responsibility: Represent authentication/permission failures only.
/// </summary>
public class UnauthorizedException : DomainException
{
    /// <summary>
    /// Error code for unauthorized access.
    /// </summary>
    public override string ErrorCode => ErrorCode.Unauthorized;

    /// <summary>
    /// Initialize with custom message.
    /// </summary>
    public UnauthorizedException(string message)
        : base(message, ErrorCode.Unauthorized)
    {
    }

    /// <summary>
    /// Initialize with user ID and required permission.
    /// </summary>
    public UnauthorizedException(string userId, string requiredPermission)
        : base($"User '{userId}' does not have permission '{requiredPermission}'.", ErrorCode.Unauthorized)
    {
    }
}
