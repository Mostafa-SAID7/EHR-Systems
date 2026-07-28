#nullable enable

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Moq;

namespace EHRPlatform.Tests.Common.Helpers;

/// <summary>
/// Utilities for creating mocks for common service interfaces.
/// </summary>
public static class MockHelper
{
    /// <summary>
    /// Create a strict mock (requires all setup, strict verification).
    /// </summary>
    public static Mock<T> CreateStrictMock<T>() where T : class
    {
        return new Mock<T>(MockBehavior.Strict);
    }

    /// <summary>
    /// Create a loose mock (allows unsetup calls with default behavior).
    /// </summary>
    public static Mock<T> CreateLooseMock<T>() where T : class
    {
        return new Mock<T>(MockBehavior.Loose);
    }

    /// <summary>
    /// Generate valid JWT token for testing.
    /// </summary>
    public static string GenerateJwtToken(
        string userId = "",
        string email = "",
        string[] roles = null!,
        int expirationMinutes = 60)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("test-secret-key-for-testing-only-1234567890"));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, string.IsNullOrEmpty(userId) ? Guid.NewGuid().ToString() : userId),
            new Claim(ClaimTypes.Email, string.IsNullOrEmpty(email) ? TestDataGenerator.GenerateEmail() : email),
        };

        if (roles != null)
        {
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var token = new JwtSecurityToken(
            issuer: "test-issuer",
            audience: "test-audience",
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Generate authorization header value.
    /// </summary>
    public static string GenerateAuthorizationHeader(string token = "")
    {
        if (string.IsNullOrEmpty(token))
            token = GenerateJwtToken();

        return $"Bearer {token}";
    }

    /// <summary>
    /// Create mock for logger.
    /// </summary>
    public static Mock<Microsoft.Extensions.Logging.ILogger<T>> CreateLoggerMock<T>()
    {
        var mock = new Mock<Microsoft.Extensions.Logging.ILogger<T>>();
        return mock;
    }

    /// <summary>
    /// Create mock for repository with basic CRUD setup.
    /// </summary>
    public static Mock<EHRPlatform.Common.Data.IRepository<T>> CreateRepositoryMock<T>() where T : class
    {
        var mock = new Mock<EHRPlatform.Common.Data.IRepository<T>>();

        // Default setup for common repository operations
        mock.Setup(x => x.AddAsync(It.IsAny<T>()))
            .ReturnsAsync((T entity) => entity);

        mock.Setup(x => x.UpdateAsync(It.IsAny<T>()))
            .ReturnsAsync((T entity) => entity);

        mock.Setup(x => x.DeleteAsync(It.IsAny<T>()))
            .ReturnsAsync(true);

        return mock;
    }

    /// <summary>
    /// Create mock for Unit of Work.
    /// </summary>
    public static Mock<EHRPlatform.Common.Data.IUnitOfWork> CreateUnitOfWorkMock()
    {
        var mock = new Mock<EHRPlatform.Common.Data.IUnitOfWork>();

        mock.Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);

        mock.Setup(x => x.BeginTransactionAsync())
            .ReturnsAsync(new Mock<System.Data.Common.DbTransaction>().Object);

        return mock;
    }

    /// <summary>
    /// Create mock for cache service.
    /// </summary>
    public static Mock<EHRPlatform.Common.Caching.ICacheService> CreateCacheServiceMock()
    {
        var mock = new Mock<EHRPlatform.Common.Caching.ICacheService>();

        mock.Setup(x => x.GetAsync<It.IsAnyType>(It.IsAny<string>()))
            .ReturnsAsync((object?)null);

        mock.Setup(x => x.SetAsync(It.IsAny<string>(), It.IsAny<object>(), It.IsAny<TimeSpan>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        mock.Setup(x => x.RemoveAsync(It.IsAny<string>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        return mock;
    }

    /// <summary>
    /// Create mock for message bus.
    /// </summary>
    public static Mock<EHRPlatform.Common.Messaging.IMessageBus> CreateMessageBusMock()
    {
        var mock = new Mock<EHRPlatform.Common.Messaging.IMessageBus>();

        mock.Setup(x => x.PublishAsync(It.IsAny<EHRPlatform.Common.Events.IDomainEvent>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        return mock;
    }

    /// <summary>
    /// Create mock for authentication context.
    /// </summary>
    public static Mock<EHRPlatform.Common.Security.IAuthenticationContext> CreateAuthenticationContextMock(
        string userId = "",
        string email = "",
        string[] roles = null!)
    {
        var mock = new Mock<EHRPlatform.Common.Security.IAuthenticationContext>();

        mock.Setup(x => x.UserId)
            .Returns(string.IsNullOrEmpty(userId) ? Guid.NewGuid().ToString() : userId);

        mock.Setup(x => x.Email)
            .Returns(string.IsNullOrEmpty(email) ? TestDataGenerator.GenerateEmail() : email);

        mock.Setup(x => x.Roles)
            .Returns(roles ?? Array.Empty<string>());

        mock.Setup(x => x.IsAuthenticated)
            .Returns(true);

        return mock;
    }

    /// <summary>
    /// Create mock for audit service.
    /// </summary>
    public static Mock<EHRPlatform.Services.Audit.Domain.Services.IAuditService> CreateAuditServiceMock()
    {
        var mock = new Mock<EHRPlatform.Services.Audit.Domain.Services.IAuditService>();

        mock.Setup(x => x.LogAccessAsync(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            It.IsAny<string>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        mock.Setup(x => x.LogChangeAsync(
            It.IsAny<string>(),
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<string>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        return mock;
    }
}
