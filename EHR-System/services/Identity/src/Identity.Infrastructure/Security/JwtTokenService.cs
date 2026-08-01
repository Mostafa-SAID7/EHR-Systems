using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EHRPlatform.Services.Identity.Domain.Entities;
using Microsoft.IdentityModel.Tokens;

namespace EHRPlatform.Services.Identity.Infrastructure.Security;

/// <summary>
/// Concrete JWT token generator using HS256.
/// </summary>
public sealed class JwtTokenService : IJwtTokenService
{
    private readonly SymmetricSecurityKey _signingKey;
    private readonly string _issuer;
    private readonly string _audience;
    private readonly int _expirationMinutes;

    public int ExpiresInSeconds => _expirationMinutes * 60;

    public JwtTokenService(string secret, string issuer, string audience, int expirationMinutes = 60)
    {
        if (string.IsNullOrWhiteSpace(secret) || secret.Length < 32)
            throw new ArgumentException("JWT secret must be at least 32 characters.", nameof(secret));

        _signingKey        = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        _issuer            = issuer;
        _audience          = audience;
        _expirationMinutes = expirationMinutes;
    }

    public string GenerateAccessToken(User user, IEnumerable<string>? roles = null)
    {
        var now     = DateTime.UtcNow;
        var expires = now.AddMinutes(_expirationMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,         user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email,        user.Email),
            new(JwtRegisteredClaimNames.GivenName,    user.FirstName),
            new(JwtRegisteredClaimNames.FamilyName,   user.LastName),
            new(JwtRegisteredClaimNames.Jti,          Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat,          new DateTimeOffset(now).ToUnixTimeSeconds().ToString(),
                                                      ClaimValueTypes.Integer64),
        };

        if (roles != null)
            foreach (var role in roles)
                claims.Add(new Claim(ClaimTypes.Role, role));

        var creds = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer:             _issuer,
            audience:           _audience,
            claims:             claims,
            notBefore:          now,
            expires:            expires,
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

