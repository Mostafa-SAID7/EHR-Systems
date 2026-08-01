#nullable enable

namespace EHRPlatform.Services.Identity.Contracts.Responses;

/// <summary>
/// Login response DTO.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// JWT access token for API authentication.
    /// </summary>
    public string AccessToken { get; set; } = string.Empty;

    /// <summary>
    /// Refresh token for obtaining new access tokens.
    /// </summary>
    public string RefreshToken { get; set; } = string.Empty;

    /// <summary>
    /// Seconds until access token expires.
    /// </summary>
    public int ExpiresIn { get; set; }

    /// <summary>
    /// Token type â€” always "Bearer".
    /// </summary>
    public string TokenType { get; set; } = "Bearer";

    /// <summary>
    /// Whether MFA is required to complete login.
    /// </summary>
    public bool MfaRequired { get; set; }

    /// <summary>
    /// Temporary session ID for MFA verification (if MfaRequired is true).
    /// </summary>
    public string? MfaSessionId { get; set; }

    /// <summary>
    /// Authenticated user profile â€” included so the frontend
    /// does not need a second /me round-trip after login.
    /// </summary>
    public UserResponseDto? User { get; set; }

    /// <summary>
    /// Convenience nested token object matching the frontend AuthTokenResponse model.
    /// </summary>
    public LoginTokenDto Token => new()
    {
        AccessToken  = AccessToken,
        RefreshToken = RefreshToken,
        ExpiresIn    = ExpiresIn,
        TokenType    = TokenType
    };
}

/// <summary>Nested token shape matching front-end AuthTokenResponse interface.</summary>
public class LoginTokenDto
{
    public string AccessToken  { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public int    ExpiresIn    { get; set; }
    public string TokenType    { get; set; } = "Bearer";
}

