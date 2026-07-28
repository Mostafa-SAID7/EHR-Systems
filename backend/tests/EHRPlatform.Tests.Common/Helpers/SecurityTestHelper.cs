using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace EHRPlatform.Tests.Common.Helpers;

/// <summary>
/// Helper class for security testing.
/// Provides utilities for JWT generation, security payload generation, and header validation.
/// </summary>
public static class SecurityTestHelper
{
    private const string DefaultSecret = "test-secret-key-for-jwt-signing-must-be-long-enough";

    /// <summary>
    /// Generates a valid JWT token with custom claims for authentication testing.
    /// </summary>
    /// <param name="claims">Custom claims to include in token.</param>
    /// <param name="secret">Secret key for signing.</param>
    /// <param name="expirationMinutes">Token expiration time in minutes.</param>
    /// <returns>A valid JWT token string.</returns>
    public static string GenerateValidJwtToken(
        Dictionary<string, object> claims = null,
        string secret = DefaultSecret,
        int expirationMinutes = 60)
    {
        var header = new Dictionary<string, string>
        {
            { "alg", "HS256" },
            { "typ", "JWT" }
        };

        var payload = new Dictionary<string, object>
        {
            { "sub", Guid.NewGuid().ToString() },
            { "email", TestDataGenerator.GenerateEmail() },
            { "name", TestDataGenerator.GenerateName() },
            { "iat", DateTimeOffset.UtcNow.ToUnixTimeSeconds() },
            { "exp", DateTimeOffset.UtcNow.AddMinutes(expirationMinutes).ToUnixTimeSeconds() }
        };

        // Add custom claims
        if (claims != null)
        {
            foreach (var claim in claims)
            {
                payload[claim.Key] = claim.Value;
            }
        }

        return CreateJwt(header, payload, secret);
    }

    /// <summary>
    /// Generates an expired JWT token for testing token expiration.
    /// </summary>
    /// <param name="claims">Custom claims to include.</param>
    /// <returns>An expired JWT token string.</returns>
    public static string GenerateExpiredJwtToken(Dictionary<string, object> claims = null)
    {
        var header = new Dictionary<string, string>
        {
            { "alg", "HS256" },
            { "typ", "JWT" }
        };

        var payload = new Dictionary<string, object>
        {
            { "sub", Guid.NewGuid().ToString() },
            { "email", TestDataGenerator.GenerateEmail() },
            { "iat", DateTimeOffset.UtcNow.AddHours(-2).ToUnixTimeSeconds() },
            { "exp", DateTimeOffset.UtcNow.AddHours(-1).ToUnixTimeSeconds() }
        };

        if (claims != null)
        {
            foreach (var claim in claims)
            {
                payload[claim.Key] = claim.Value;
            }
        }

        return CreateJwt(header, payload, DefaultSecret);
    }

    /// <summary>
    /// Generates an invalid JWT token (tampered payload).
    /// </summary>
    /// <returns>An invalid JWT token string.</returns>
    public static string GenerateInvalidJwtToken()
    {
        var validToken = GenerateValidJwtToken();
        var parts = validToken.Split('.');
        
        // Tamper with the payload
        var originalPayload = parts[1];
        var tamperedPayload = Convert.ToBase64String(Encoding.UTF8.GetBytes("tampered-payload"));
        
        return $"{parts[0]}.{tamperedPayload}.{parts[2]}";
    }

    /// <summary>
    /// Generates a JWT token with insufficient permissions.
    /// </summary>
    /// <returns>A JWT token with limited claims.</returns>
    public static string GenerateLimitedPermissionsJwtToken()
    {
        var claims = new Dictionary<string, object>
        {
            { "role", "user" },
            { "permissions", new[] { "read:own-data" } }
        };

        return GenerateValidJwtToken(claims);
    }

    /// <summary>
    /// Generates a JWT token with admin permissions.
    /// </summary>
    /// <returns>A JWT token with admin claims.</returns>
    public static string GenerateAdminJwtToken()
    {
        var claims = new Dictionary<string, object>
        {
            { "role", "admin" },
            { "permissions", new[] { "read:all", "write:all", "delete:all", "manage:users" } }
        };

        return GenerateValidJwtToken(claims);
    }

    /// <summary>
    /// Generates SQL injection test payloads.
    /// </summary>
    /// <returns>List of SQL injection payloads for testing.</returns>
    public static List<string> GenerateSqlInjectionPayloads()
    {
        return new List<string>
        {
            "'; DROP TABLE users; --",
            "1' OR '1'='1",
            "admin' --",
            "' OR 1=1 --",
            "' UNION SELECT NULL, NULL, NULL --",
            "1'; UPDATE users SET admin=1; --",
            "' OR 'a'='a",
            "1' AND '1'='1",
            "' OR '1'='1' /*",
            "1' OR 1=1 --"
        };
    }

    /// <summary>
    /// Generates XSS (Cross-Site Scripting) test payloads.
    /// </summary>
    /// <returns>List of XSS payloads for testing.</returns>
    public static List<string> GenerateXssPayloads()
    {
        return new List<string>
        {
            "<script>alert('XSS')</script>",
            "<img src=x onerror=alert('XSS')>",
            "<svg onload=alert('XSS')>",
            "javascript:alert('XSS')",
            "<iframe src=javascript:alert('XSS')></iframe>",
            "<body onload=alert('XSS')>",
            "<input onfocus=alert('XSS') autofocus>",
            "<select onfocus=alert('XSS') autofocus>",
            "<textarea onfocus=alert('XSS') autofocus>",
            "<marquee onstart=alert('XSS')>",
            "\"><script>alert('XSS')</script>",
            "<img src= onerror=alert('XSS')>"
        };
    }

    /// <summary>
    /// Generates CSRF token for testing cross-site request forgery prevention.
    /// </summary>
    /// <returns>A CSRF token string.</returns>
    public static string GenerateCsrfToken()
    {
        var random = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(random);
        }
        return Convert.ToBase64String(random);
    }

    /// <summary>
    /// Validates security headers in HTTP response.
    /// </summary>
    /// <param name="headers">Dictionary of response headers.</param>
    /// <returns>True if all required security headers are present.</returns>
    public static bool ValidateSecurityHeaders(Dictionary<string, string> headers)
    {
        if (headers == null)
            return false;

        var requiredHeaders = new Dictionary<string, string>
        {
            { "X-Content-Type-Options", "nosniff" },
            { "X-Frame-Options", "DENY" },
            { "X-XSS-Protection", "1; mode=block" },
            { "Strict-Transport-Security", "" } // Any value is acceptable
        };

        foreach (var required in requiredHeaders)
        {
            if (!headers.ContainsKey(required.Key))
                return false;

            if (!string.IsNullOrEmpty(required.Value) && headers[required.Key] != required.Value)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Creates proper Authorization header for Bearer token.
    /// </summary>
    /// <param name="token">The JWT token.</param>
    /// <returns>Authorization header value.</returns>
    public static string CreateAuthorizationHeader(string token) => $"Bearer {token}";

    /// <summary>
    /// Generates basic authentication credentials.
    /// </summary>
    /// <param name="username">Username.</param>
    /// <param name="password">Password.</param>
    /// <returns>Base64 encoded basic auth header value.</returns>
    public static string GenerateBasicAuthHeader(string username, string password)
    {
        var credentials = $"{username}:{password}";
        var encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
        return $"Basic {encoded}";
    }

    /// <summary>
    /// Validates strong password requirements.
    /// </summary>
    /// <param name="password">Password to validate.</param>
    /// <returns>True if password meets security requirements.</returns>
    public static bool ValidatePasswordStrength(string password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < 12)
            return false;

        var hasUppercase = System.Text.RegularExpressions.Regex.IsMatch(password, @"[A-Z]");
        var hasLowercase = System.Text.RegularExpressions.Regex.IsMatch(password, @"[a-z]");
        var hasDigit = System.Text.RegularExpressions.Regex.IsMatch(password, @"\d");
        var hasSpecialChar = System.Text.RegularExpressions.Regex.IsMatch(password, @"[!@#$%^&*()_+\-=\[\]{};':""\\|,.<>\/?]");

        return hasUppercase && hasLowercase && hasDigit && hasSpecialChar;
    }

    /// <summary>
    /// Generates an API key for testing API authentication.
    /// </summary>
    /// <returns>A randomly generated API key.</returns>
    public static string GenerateApiKey()
    {
        var random = new byte[32];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(random);
        }
        return $"sk_{Convert.ToBase64String(random).Replace("+", "-").Replace("/", "_").TrimEnd('=')}";
    }

    /// <summary>
    /// Generates rate limiting test headers.
    /// </summary>
    /// <param name="remaining">Requests remaining.</param>
    /// <param name="limit">Request limit.</param>
    /// <param name="resetTime">Time when limit resets (Unix timestamp).</param>
    /// <returns>Dictionary of rate limit headers.</returns>
    public static Dictionary<string, string> GenerateRateLimitHeaders(int remaining = 100, int limit = 1000, long resetTime = 0)
    {
        if (resetTime == 0)
            resetTime = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();

        return new Dictionary<string, string>
        {
            { "X-RateLimit-Limit", limit.ToString() },
            { "X-RateLimit-Remaining", remaining.ToString() },
            { "X-RateLimit-Reset", resetTime.ToString() }
        };
    }

    /// <summary>
    /// Helper to create JWT token.
    /// </summary>
    private static string CreateJwt(Dictionary<string, string> header, Dictionary<string, object> payload, string secret)
    {
        var headerJson = JsonSerializer.Serialize(header);
        var payloadJson = JsonSerializer.Serialize(payload);

        var headerEncoded = Base64UrlEncode(Encoding.UTF8.GetBytes(headerJson));
        var payloadEncoded = Base64UrlEncode(Encoding.UTF8.GetBytes(payloadJson));

        var message = $"{headerEncoded}.{payloadEncoded}";

        using (var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret)))
        {
            var signature = Base64UrlEncode(hmac.ComputeHash(Encoding.UTF8.GetBytes(message)));
            return $"{message}.{signature}";
        }
    }

    /// <summary>
    /// Helper to encode bytes to Base64Url format.
    /// </summary>
    private static string Base64UrlEncode(byte[] input)
    {
        var output = Convert.ToBase64String(input);
        output = output.Split('=')[0];
        output = output.Replace('+', '-');
        output = output.Replace('/', '_');
        return output;
    }
}
