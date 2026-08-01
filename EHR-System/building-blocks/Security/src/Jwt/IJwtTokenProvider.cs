using System.Collections.Generic;
using System.Security.Claims;

namespace EHRPlatform.Security.Jwt;

/// <summary>
/// Interface for JWT token generation and validation.
/// Single responsibility: JWT token service contract.
/// </summary>
public interface IJwtTokenProvider
{
    /// <summary>
    /// Generate JWT access token.
    /// </summary>
    string GenerateAccessToken(string userId, string userName, string email, List<string> roles);

    /// <summary>
    /// Generate JWT refresh token (longer expiration).
    /// </summary>
    string GenerateRefreshToken(string userId, string userName, string email);

    /// <summary>
    /// Validate JWT token and extract claims.
    /// </summary>
    (bool IsValid, ClaimsPrincipal? Principal, string? Error) ValidateToken(string token);

    /// <summary>
    /// Extract claim value from token string.
    /// </summary>
    string? GetClaimFromToken(string token, string claimType);

    /// <summary>
    /// Check if token is expired.
    /// </summary>
    bool IsTokenExpired(string token);
}
