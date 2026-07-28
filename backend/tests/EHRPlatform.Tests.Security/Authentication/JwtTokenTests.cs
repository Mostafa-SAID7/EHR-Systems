#nullable enable

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Xunit;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using EHRPlatform.Tests.Common.Helpers;

namespace EHRPlatform.Tests.Security.Authentication;

/// <summary>
/// Security tests for JWT token generation and validation.
/// Tests authentication token handling and security properties.
/// </summary>
public class JwtTokenTests
{
    private const string TestSecret = "test-secret-key-for-testing-only-1234567890";

    [Fact]
    public void GenerateToken_WithValidClaims_IsValid()
    {
        // Arrange & Act
        var token = MockHelper.GenerateJwtToken(
            userId: "test-user-id",
            email: "test@test.com",
            roles: new[] { "User" });

        // Assert
        token.Should().NotBeEmpty();
        token.Should().StartWith("eyJ"); // Valid JWT header
        var parts = token.Split('.');
        parts.Should().HaveLength(3); // Header.Payload.Signature
    }

    [Fact]
    public void JwtToken_Contains_RequiredClaims()
    {
        // Arrange
        var userId = Guid.NewGuid().ToString();
        var email = "user@test.com";

        // Act
        var token = MockHelper.GenerateJwtToken(userId, email);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.Claims.Should().NotBeEmpty();
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.NameIdentifier);
        jwtToken.Claims.Should().Contain(c => c.Type == ClaimTypes.Email);
    }

    [Fact]
    public void JwtToken_WithRole_IncludesRoleClaim()
    {
        // Arrange & Act
        var token = MockHelper.GenerateJwtToken(
            roles: new[] { "Admin", "Doctor" });

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        var roleClaims = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).ToList();
        roleClaims.Should().HaveCount(2);
        roleClaims.Should().Contain(c => c.Value == "Admin");
        roleClaims.Should().Contain(c => c.Value == "Doctor");
    }

    [Fact]
    public void JwtToken_ExpiresAt_CorrectTime()
    {
        // Arrange
        var expirationMinutes = 60;

        // Act
        var token = MockHelper.GenerateJwtToken(expirationMinutes: expirationMinutes);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.ValidTo.Should().BeAfter(DateTime.UtcNow);
        jwtToken.ValidTo.Should().BeBefore(DateTime.UtcNow.AddMinutes(expirationMinutes + 1));
    }

    [Fact]
    public void AuthorizationHeader_Format_IsCorrect()
    {
        // Arrange & Act
        var token = MockHelper.GenerateJwtToken();
        var header = MockHelper.GenerateAuthorizationHeader(token);

        // Assert
        header.Should().StartWith("Bearer ");
        header.Should().Contain(token);
    }

    [Fact]
    public void InvalidToken_DecodingFails()
    {
        // Arrange
        var invalidToken = "invalid-token-123";

        // Act & Assert
        var handler = new JwtSecurityTokenHandler();
        var action = () => handler.ReadJwtToken(invalidToken);

        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ExpiredToken_IsInvalid()
    {
        // Arrange
        var token = MockHelper.GenerateJwtToken(expirationMinutes: -1);

        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.ValidTo.Should().BeBefore(DateTime.UtcNow);
    }

    [Fact]
    public void TokenWithoutSignature_IsInvalid()
    {
        // Arrange
        var invalidToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIn0.invalid";

        var handler = new JwtSecurityTokenHandler();

        // Act & Assert
        var action = () => handler.ReadJwtToken(invalidToken);
        action.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void TokenIssuer_IsCorrect()
    {
        // Arrange & Act
        var token = MockHelper.GenerateJwtToken();
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.Issuer.Should().Be("test-issuer");
    }

    [Fact]
    public void TokenAudience_IsCorrect()
    {
        // Arrange & Act
        var token = MockHelper.GenerateJwtToken();
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        jwtToken.Audiences.Should().Contain("test-audience");
    }

    [Theory]
    [InlineData(60)]
    [InlineData(120)]
    [InlineData(1)]
    public void TokenExpiration_IsConfigurable(int minutes)
    {
        // Arrange & Act
        var token = MockHelper.GenerateJwtToken(expirationMinutes: minutes);
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);

        // Assert
        var expectedExpiration = DateTime.UtcNow.AddMinutes(minutes);
        jwtToken.ValidTo.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void DifferentTokens_HaveDifferentSignatures()
    {
        // Arrange & Act
        var token1 = MockHelper.GenerateJwtToken(userId: "user1");
        var token2 = MockHelper.GenerateJwtToken(userId: "user2");

        // Assert
        token1.Should().NotBe(token2);
    }

    [Fact]
    public void TokenPayload_IsNotEncrypted()
    {
        // Arrange
        var userId = "test-user-123";

        // Act
        var token = MockHelper.GenerateJwtToken(userId: userId);
        var parts = token.Split('.');
        var payload = parts[1];

        // Decode base64
        var padding = payload.Length % 4;
        if (padding > 0)
            payload += new string('=', 4 - padding);

        var decoded = Encoding.UTF8.GetString(Convert.FromBase64String(payload));

        // Assert - JWT payload is base64 encoded but not encrypted
        decoded.Should().Contain(userId);
    }
}
