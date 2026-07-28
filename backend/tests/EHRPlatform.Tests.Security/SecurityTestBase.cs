using System;
using Xunit;

namespace EHRPlatform.Tests.Security;

/// <summary>
/// Base class for security tests
/// </summary>
public abstract class SecurityTestBase
{
    protected const string ValidToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c";
    protected const string InvalidToken = "invalid.token.here";
    protected const string ExpiredToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyLCJleHAiOjE1MTYyMzkwMjJ9.5mpsXHiYvFTPu3L0W7YzKGsRIYpjEHEWXXHZOg9dN5s";

    protected string GenerateTestToken(string userId, string role = "User")
    {
        return $"token_for_{userId}_{role}";
    }

    protected void ValidateSecurityHeaders(Dictionary<string, string> headers)
    {
        Assert.NotEmpty(headers);
        
        var expectedHeaders = new[]
        {
            "X-Content-Type-Options",
            "X-Frame-Options",
            "X-XSS-Protection",
            "Strict-Transport-Security",
            "Content-Security-Policy"
        };

        foreach (var header in expectedHeaders)
        {
            Assert.Contains(header, headers.Keys);
        }
    }
}
