namespace Identity.Domain.Enums;

/// <summary>
/// Enumeration for token types
/// </summary>
public enum TokenType
{
    /// <summary>
    /// Access token for API authentication
    /// </summary>
    AccessToken = 1,

    /// <summary>
    /// Refresh token for obtaining new access tokens
    /// </summary>
    RefreshToken = 2,

    /// <summary>
    /// Email verification token
    /// </summary>
    EmailVerification = 3,

    /// <summary>
    /// Password reset token
    /// </summary>
    PasswordReset = 4,

    /// <summary>
    /// Multi-factor authentication token
    /// </summary>
    MfaToken = 5
}
