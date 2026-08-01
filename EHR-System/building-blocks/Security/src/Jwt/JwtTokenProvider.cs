using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace EHRPlatform.Security.Jwt;

/// <summary>
/// JWT token generation and validation provider.
/// </summary>
public class JwtTokenProvider
{
    private readonly JwtSettings _settings;

    public JwtTokenProvider(JwtSettings settings)
    {
        _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    }

    /// <summary>
    /// Generate JWT access token.
    /// </summary>
    public string GenerateAccessToken(
        string userId,
        string userName,
        string email,
        List<string> roles)
    {
        return GenerateToken(
            userId,
            userName,
            email,
            roles,
            _settings.AccessTokenExpirationMinutes * 60); // Convert to seconds
    }

    /// <summary>
    /// Generate JWT refresh token (longer expiration).
    /// </summary>
    public string GenerateRefreshToken(
        string userId,
        string userName,
        string email)
    {
        return GenerateToken(
            userId,
            userName,
            email,
            new List<string>(),
            _settings.RefreshTokenExpirationDays * 24 * 3600); // Convert to seconds
    }

    /// <summary>
    /// Generate JWT token with custom expiration.
    /// </summary>
    private string GenerateToken(
        string userId,
        string userName,
        string email,
        List<string> roles,
        long expirationSeconds)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Name, userName),
            new Claim(ClaimTypes.Email, email),
            new Claim("userId", userId),
        };

        // Add roles as claims
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddSeconds(expirationSeconds),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Validate JWT token and extract claims.
    /// </summary>
    public (bool IsValid, ClaimsPrincipal? Principal, string? Error) ValidateToken(string token)
    {
        try
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
            var tokenHandler = new JwtSecurityTokenHandler();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = _settings.ValidateIssuerSigningKey,
                IssuerSigningKey = key,
                ValidateIssuer = _settings.ValidateIssuer,
                ValidIssuer = _settings.Issuer,
                ValidateAudience = _settings.ValidateAudience,
                ValidAudience = _settings.Audience,
                ValidateLifetime = _settings.ValidateLifetime,
                ClockSkew = TimeSpan.FromSeconds(_settings.ClockSkewSeconds)
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
            return (true, principal, null);
        }
        catch (SecurityTokenException ex)
        {
            return (false, null, $"Token validation failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            return (false, null, $"Unexpected error: {ex.Message}");
        }
    }

    /// <summary>
    /// Extract claim value from token string.
    /// </summary>
    public string? GetClaimFromToken(string token, string claimType)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.Claims.FirstOrDefault(c => c.Type == claimType)?.Value;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Check if token is expired.
    /// </summary>
    public bool IsTokenExpired(string token)
    {
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var jwtToken = handler.ReadJwtToken(token);
            return jwtToken.ValidTo < DateTime.UtcNow;
        }
        catch
        {
            return true;
        }
    }
}
