namespace Identity.Domain.Enums;

/// <summary>
/// Enumeration for user account status
/// </summary>
public enum UserStatus
{
    /// <summary>
    /// User account is active and can login
    /// </summary>
    Active = 1,

    /// <summary>
    /// User account is suspended and cannot login
    /// </summary>
    Suspended = 2,

    /// <summary>
    /// User account is disabled
    /// </summary>
    Disabled = 3,

    /// <summary>
    /// User account is pending email verification
    /// </summary>
    PendingEmailVerification = 4,

    /// <summary>
    /// User account is locked due to failed login attempts
    /// </summary>
    LockedOut = 5
}
