using EHRPlatform.Services.Identity.Domain.Entities;

namespace EHRPlatform.Services.Identity.Infrastructure.Security;

/// <summary>
/// Generates and validates JWT access tokens for the Identity service.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Generate a signed JWT access token for the given user.
    /// Includes standard claims: sub, email, given_name, family_name, roles, jti.
    /// </summary>
    string GenerateAccessToken(User user, IEnumerable<string>? roles = null);

    /// <summary>
    /// Expiration in seconds embedded in the token (mirrors ExpirationMinutes * 60).
    /// </summary>
    int ExpiresInSeconds { get; }
}

