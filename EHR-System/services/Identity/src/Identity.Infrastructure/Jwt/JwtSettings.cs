using System;

namespace Identity.Infrastructure.Jwt;

/// <summary>
/// JWT configuration settings.
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// JWT issuer (service that issues the token).
    /// </summary>
    public string Issuer { get; set; } = null!;

    /// <summary>
    /// JWT audience (services that accept this token).
    /// </summary>
    public string Audience { get; set; } = null!;

    /// <summary>
    /// Secret key for signing tokens (HS256).
    /// Must be at least 32 characters for HS256.
    /// </summary>
    public string SecretKey { get; set; } = null!;

    /// <summary>
    /// Access token expiration in minutes.
    /// Default: 60 minutes.
    /// </summary>
    public int AccessTokenExpirationMinutes { get; set; } = 60;

    /// <summary>
    /// Refresh token expiration in days.
    /// Default: 7 days.
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; } = 7;

    /// <summary>
    /// Algorithm for signing (HS256, RS256, etc.).
    /// Default: HS256.
    /// </summary>
    public string Algorithm { get; set; } = "HS256";

    /// <summary>
    /// Whether to validate issuer.
    /// Default: true.
    /// </summary>
    public bool ValidateIssuer { get; set; } = true;

    /// <summary>
    /// Whether to validate audience.
    /// Default: true.
    /// </summary>
    public bool ValidateAudience { get; set; } = true;

    /// <summary>
    /// Whether to validate token signature.
    /// Default: true.
    /// </summary>
    public bool ValidateIssuerSigningKey { get; set; } = true;

    /// <summary>
    /// Whether to validate token lifetime.
    /// Default: true.
    /// </summary>
    public bool ValidateLifetime { get; set; } = true;

    /// <summary>
    /// Clock skew tolerance in seconds (for time synchronization).
    /// Default: 0 seconds.
    /// </summary>
    public int ClockSkewSeconds { get; set; } = 0;
}
