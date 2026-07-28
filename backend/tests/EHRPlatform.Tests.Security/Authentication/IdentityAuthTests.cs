using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EHRPlatform.Services.Identity.Domain.Entities;
using EHRPlatform.Services.Identity.Security;
using EHRPlatform.Tests.Common.Base;
using FluentAssertions;
using Microsoft.IdentityModel.Tokens;
using Xunit;

namespace EHRPlatform.Tests.Security.Authentication;

/// <summary>
/// Security tests for JWT token generation and validation.
/// Tests token claims, expiration, signature verification, HIPAA compliance.
/// </summary>
public class IdentityAuthTests : UnitTestBase
{
    private const string TestSecret = "SuperSecretKeyThatIsAtLeast32CharactersLong!@#$%";
    private const string TestIssuer = "EHR-Platform";
    private const string TestAudience = "EHR-Services";
    private readonly IJwtTokenService _jwtTokenService;

    public IdentityAuthTests()
    {
        _jwtTokenService = new JwtTokenService(TestSecret, TestIssuer, TestAudience, expirationMinutes: 60);
    }

    #region JWT Token Generation Tests

    [Fact]
    public void GenerateAccessToken_WithValidUser_ShouldReturnSignedToken()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe",
            IsActive = true
        };

        // Act
        var token = _jwtTokenService.GenerateAccessToken(user);

        // Assert
        token.Should().NotBeEmpty();
        token.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GenerateAccessToken_ShouldIncludeRequiredClaims()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "claims@example.com",
            FirstName = "Claims",
            LastName = "Test",
            IsActive = true
        };

        // Act
        var token = _jwtTokenService.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var decodedToken = handler.ReadToken(token) as JwtSecurityToken;

        // Assert
        decodedToken.Should().NotBeNull();
        decodedToken!.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Sub && c.Value == userId.ToString());
        decodedToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Email && c.Value == "claims@example.com");
        decodedToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.GivenName && c.Value == "Claims");
        decodedToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.FamilyName && c.Value == "Test");
        decodedToken.Claims.Should().Contain(c => c.Type == JwtRegisteredClaimNames.Jti); // JWT ID
    }

    [Fact]
    public void GenerateAccessToken_WithRoles_ShouldIncludeRoleClaims()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "roles@example.com",
            FirstName = "Role",
            LastName = "Test",
            IsActive = true
        };

        var roles = new[] { "Doctor", "Admin" };

        // Act
        var token = _jwtTokenService.GenerateAccessToken(user, roles);
        var handler = new JwtSecurityTokenHandler();
        var decodedToken = handler.ReadToken(token) as JwtSecurityToken;

        // Assert
        decodedToken!.Claims
            .Where(c => c.Type == ClaimTypes.Role)
            .Select(c => c.Value)
            .Should()
            .Contain(new[] { "Doctor", "Admin" });
    }

    [Fact]
    public void GenerateAccessToken_ShouldHaveCorrectIssuer()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "issuer@example.com",
            FirstName = "Issuer",
            LastName = "Test",
            IsActive = true
        };

        // Act
        var token = _jwtTokenService.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var decodedToken = handler.ReadToken(token) as JwtSecurityToken;

        // Assert
        decodedToken!.Issuer.Should().Be(TestIssuer);
    }

    [Fact]
    public void GenerateAccessToken_ShouldHaveCorrectAudience()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "audience@example.com",
            FirstName = "Audience",
            LastName = "Test",
            IsActive = true
        };

        // Act
        var token = _jwtTokenService.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var decodedToken = handler.ReadToken(token) as JwtSecurityToken;

        // Assert
        decodedToken!.Audiences.Should().Contain(TestAudience);
    }

    [Fact]
    public void GenerateAccessToken_TokenShouldBeValidForExpectedDuration()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "expiration@example.com",
            FirstName = "Exp",
            LastName = "Test",
            IsActive = true
        };

        var beforeGeneration = DateTime.UtcNow;

        // Act
        var token = _jwtTokenService.GenerateAccessToken(user);
        var handler = new JwtSecurityTokenHandler();
        var decodedToken = handler.ReadToken(token) as JwtSecurityToken;

        var afterGeneration = DateTime.UtcNow;

        // Assert - Token should expire in approximately 60 minutes (3600 seconds)
        var expirationTime = decodedToken!.ValidTo;
        var expectedExpiration = beforeGeneration.AddMinutes(60);

        expirationTime.Should().BeCloseTo(expectedExpiration, TimeSpan.FromSeconds(5));
    }

    #endregion

    #region Token Validation Tests

    [Fact]
    public void ValidateToken_WithValidSignature_ShouldSucceed()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "valid@example.com",
            FirstName = "Valid",
            LastName = "Token",
            IsActive = true
        };

        var token = _jwtTokenService.GenerateAccessToken(user);

        // Act
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecret));
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = TestIssuer,
            ValidateAudience = true,
            ValidAudience = TestAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // Assert
        var principal = handler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
        validatedToken.Should().NotBeNull();
        principal.Identity!.IsAuthenticated.Should().BeTrue();
    }

    [Fact]
    public void ValidateToken_WithTamperedSignature_ShouldFail()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "tampered@example.com",
            FirstName = "Tampered",
            LastName = "Token",
            IsActive = true
        };

        var token = _jwtTokenService.GenerateAccessToken(user);
        var tamperedToken = token.Substring(0, token.Length - 5) + "XXXXX"; // Tamper with signature

        // Act
        var handler = new JwtSecurityTokenHandler();
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(TestSecret));
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = true,
            ValidIssuer = TestIssuer,
            ValidateAudience = true,
            ValidAudience = TestAudience,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };

        // Assert
        Assert.Throws<SecurityTokenInvalidSignatureException>(() =>
            handler.ValidateToken(tamperedToken, validationParameters, out SecurityToken _));
    }

    [Fact]
    public void ValidateToken_WithWrongSecret_ShouldFail()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "wrong@example.com",
            FirstName = "Wrong",
            LastName = "Secret",
            IsActive = true
        };

        var token = _jwtTokenService.GenerateAccessToken(user);

        // Act
        var handler = new JwtSecurityTokenHandler();
        var wrongSecret = "DifferentSecretKeyThatIsAtLeast32CharactersLongXXX!@#";
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(wrongSecret));
        var validationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = key,
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = false,
            ClockSkew = TimeSpan.Zero
        };

        // Assert
        Assert.Throws<SecurityTokenInvalidSignatureException>(() =>
            handler.ValidateToken(token, validationParameters, out SecurityToken _));
    }

    #endregion

    #region Security Best Practices Tests

    [Fact]
    public void JwtTokenService_RequiresMinimum32CharacterSecret()
    {
        // Arrange
        var shortSecret = "short";

        // Act & Assert
        Assert.Throws<ArgumentException>(() => 
            new JwtTokenService(shortSecret, TestIssuer, TestAudience));
    }

    [Fact]
    public void JwtTokenService_RequiresNonNullSecret()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(() =>
            new JwtTokenService(null!, TestIssuer, TestAudience));
    }

    [Fact]
    public void GenerateAccessToken_ShouldHaveJti_UniquePerId()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "jti@example.com",
            FirstName = "JTI",
            LastName = "Test",
            IsActive = true
        };

        // Act
        var token1 = _jwtTokenService.GenerateAccessToken(user);
        var token2 = _jwtTokenService.GenerateAccessToken(user);

        var handler = new JwtSecurityTokenHandler();
        var decoded1 = handler.ReadToken(token1) as JwtSecurityToken;
        var decoded2 = handler.ReadToken(token2) as JwtSecurityToken;

        var jti1 = decoded1!.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;
        var jti2 = decoded2!.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Jti)?.Value;

        // Assert
        jti1.Should().NotBeNull();
        jti2.Should().NotBeNull();
        jti1.Should().NotBe(jti2); // Each token should have unique JTI
    }

    #endregion
}
