using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data.Abstractions;
using EHRPlatform.Common.Domain.Exceptions;
using EHRPlatform.Common.Infrastructure.Security;
using EHRPlatform.Services.Identity.Application.Identity.DTOs.Responses;
using EHRPlatform.Services.Identity.Domain.Entities;
using EHRPlatform.Services.Identity.Features.Auth.Commands;
using EHRPlatform.Services.Identity.Features.Auth.Handlers;
using EHRPlatform.Services.Identity.Security;
using EHRPlatform.Tests.Common.Base;
using EHRPlatform.Tests.Common.Helpers;
using FluentAssertions;
using Moq;
using Xunit;

namespace EHRPlatform.Tests.Unit.Services;

/// <summary>
/// Unit tests for Identity service handlers.
/// Tests authentication flows: login, registration, token refresh with mocked dependencies.
/// </summary>
public class IdentityServiceTests : UnitTestBase
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IJwtTokenService> _mockJwtTokenService;
    private readonly Mock<ILogger<LoginCommandHandler>> _mockLoggerLogin;
    private readonly Mock<ILogger<RegisterCommandHandler>> _mockLoggerRegister;
    private readonly Mock<ILogger<RefreshTokenCommandHandler>> _mockLoggerRefresh;

    public IdentityServiceTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockJwtTokenService = new Mock<IJwtTokenService>();
        _mockLoggerLogin = new Mock<ILogger<LoginCommandHandler>>();
        _mockLoggerRegister = new Mock<ILogger<RegisterCommandHandler>>();
        _mockLoggerRefresh = new Mock<ILogger<RefreshTokenCommandHandler>>();
    }

    #region LoginCommandHandler Tests

    [Fact]
    public async Task LoginCommandHandler_WithValidCredentials_ShouldReturnAccessToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "user@example.com";
        var password = "Password123!";
        var passwordHash = "hashed_password";
        var passwordSalt = "salt";

        var user = new User
        {
            Id = userId,
            Email = email,
            FirstName = "John",
            LastName = "Doe",
            PasswordHash = passwordHash,
            PasswordSalt = passwordSalt,
            IsActive = true,
            MfaEnabled = false,
            FailedLoginAttempts = 0
        };

        var userRepoMock = new Mock<IRepository<User>>();
        userRepoMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Func<IQueryable<User>, IQueryable<User>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var refreshTokenRepoMock = new Mock<IRepository<RefreshToken>>();

        _mockUow
            .Setup(u => u.Repository<User>())
            .Returns(userRepoMock.Object);

        _mockUow
            .Setup(u => u.Repository<RefreshToken>())
            .Returns(refreshTokenRepoMock.Object);

        _mockPasswordHasher
            .Setup(p => p.Verify(password, passwordHash, passwordSalt))
            .Returns(true);

        _mockJwtTokenService
            .Setup(j => j.GenerateAccessToken(It.IsAny<User>(), null))
            .Returns("valid_access_token");

        _mockJwtTokenService
            .Setup(j => j.ExpiresInSeconds)
            .Returns(3600);

        var command = new LoginCommand { Email = email, Password = password };
        var handler = new LoginCommandHandler(_mockUow.Object, _mockPasswordHasher.Object, _mockJwtTokenService.Object, _mockLoggerLogin.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("valid_access_token");
        result.ExpiresIn.Should().Be(3600);
        result.MfaRequired.Should().BeFalse();
        result.User.Should().NotBeNull();
        result.User.Email.Should().Be(email);
    }

    [Fact]
    public async Task LoginCommandHandler_WithInvalidEmail_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var userRepoMock = new Mock<IRepository<User>>();
        userRepoMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Func<IQueryable<User>, IQueryable<User>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        _mockUow
            .Setup(u => u.Repository<User>())
            .Returns(userRepoMock.Object);

        var command = new LoginCommand { Email = "unknown@example.com", Password = "Password123!" };
        var handler = new LoginCommandHandler(_mockUow.Object, _mockPasswordHasher.Object, _mockJwtTokenService.Object, _mockLoggerLogin.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task LoginCommandHandler_WithInvalidPassword_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = new User
        {
            Email = "user@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsActive = true,
            MfaEnabled = false,
            FailedLoginAttempts = 0
        };

        var userRepoMock = new Mock<IRepository<User>>();
        userRepoMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Func<IQueryable<User>, IQueryable<User>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUow
            .Setup(u => u.Repository<User>())
            .Returns(userRepoMock.Object);

        _mockPasswordHasher
            .Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(false);

        var command = new LoginCommand { Email = "user@example.com", Password = "WrongPassword" };
        var handler = new LoginCommandHandler(_mockUow.Object, _mockPasswordHasher.Object, _mockJwtTokenService.Object, _mockLoggerLogin.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task LoginCommandHandler_WithLockedAccount_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var user = new User
        {
            Email = "user@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsActive = true,
            LockoutEnd = DateTime.UtcNow.AddMinutes(10)
        };

        var userRepoMock = new Mock<IRepository<User>>();
        userRepoMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Func<IQueryable<User>, IQueryable<User>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUow
            .Setup(u => u.Repository<User>())
            .Returns(userRepoMock.Object);

        var command = new LoginCommand { Email = "user@example.com", Password = "Password123!" };
        var handler = new LoginCommandHandler(_mockUow.Object, _mockPasswordHasher.Object, _mockJwtTokenService.Object, _mockLoggerLogin.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task LoginCommandHandler_WithMfaEnabled_ShouldReturnMfaRequired()
    {
        // Arrange
        var user = new User
        {
            Email = "user@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            IsActive = true,
            MfaEnabled = true,
            FailedLoginAttempts = 0
        };

        var userRepoMock = new Mock<IRepository<User>>();
        userRepoMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Func<IQueryable<User>, IQueryable<User>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUow
            .Setup(u => u.Repository<User>())
            .Returns(userRepoMock.Object);

        _mockPasswordHasher
            .Setup(p => p.Verify(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true);

        var command = new LoginCommand { Email = "user@example.com", Password = "Password123!" };
        var handler = new LoginCommandHandler(_mockUow.Object, _mockPasswordHasher.Object, _mockJwtTokenService.Object, _mockLoggerLogin.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.MfaRequired.Should().BeTrue();
        result.AccessToken.Should().BeEmpty();
    }

    #endregion

    #region RegisterCommandHandler Tests

    [Fact]
    public async Task RegisterCommandHandler_WithValidData_ShouldCreateUser()
    {
        // Arrange
        var email = "newuser@example.com";
        var firstName = "John";
        var lastName = "Doe";
        var password = "SecurePass123!";
        var hash = "hashed";
        var salt = "salt";

        var userRepoMock = new Mock<IRepository<User>>();
        userRepoMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Func<IQueryable<User>, IQueryable<User>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User)null);

        userRepoMock
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUow
            .Setup(u => u.Repository<User>())
            .Returns(userRepoMock.Object);

        _mockUow
            .Setup(u => u.SaveChangesWithEventPublishingAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockPasswordHasher
            .Setup(p => p.HashWithSalt(password))
            .Returns((hash, salt));

        var mockEncryptionService = new Mock<IEncryptionService>();

        var command = new RegisterCommand
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            Password = password
        };

        var handler = new RegisterCommandHandler(_mockUow.Object, _mockPasswordHasher.Object, mockEncryptionService.Object, _mockLoggerRegister.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Email.Should().Be(email);
        userRepoMock.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterCommandHandler_WithDuplicateEmail_ShouldThrowConflictException()
    {
        // Arrange
        var existingUser = new User { Email = "existing@example.com" };

        var userRepoMock = new Mock<IRepository<User>>();
        userRepoMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Func<IQueryable<User>, IQueryable<User>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingUser);

        _mockUow
            .Setup(u => u.Repository<User>())
            .Returns(userRepoMock.Object);

        var mockEncryptionService = new Mock<IEncryptionService>();

        var command = new RegisterCommand
        {
            Email = "existing@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "SecurePass123!"
        };

        var handler = new RegisterCommandHandler(_mockUow.Object, _mockPasswordHasher.Object, mockEncryptionService.Object, _mockLoggerRegister.Object);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => handler.Handle(command, CancellationToken.None));
    }

    #endregion

    #region RefreshTokenCommandHandler Tests

    [Fact]
    public async Task RefreshTokenCommandHandler_WithValidRefreshToken_ShouldReturnNewAccessToken()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = "valid_refresh_token";
        var hashedToken = "hashed_token";

        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            IsActive = true,
            FailedLoginAttempts = 0
        };

        var refreshTokenEntity = new RefreshToken
        {
            UserId = userId,
            Token = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        var refreshTokenRepoMock = new Mock<IRepository<RefreshToken>>();
        refreshTokenRepoMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Func<IQueryable<RefreshToken>, IQueryable<RefreshToken>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshTokenEntity);

        var userRepoMock = new Mock<IRepository<User>>();
        userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockUow
            .Setup(u => u.Repository<RefreshToken>())
            .Returns(refreshTokenRepoMock.Object);

        _mockUow
            .Setup(u => u.Repository<User>())
            .Returns(userRepoMock.Object);

        _mockPasswordHasher
            .Setup(p => p.Hash(refreshToken, string.Empty))
            .Returns(hashedToken);

        _mockJwtTokenService
            .Setup(j => j.GenerateAccessToken(user))
            .Returns("new_access_token");

        _mockJwtTokenService
            .Setup(j => j.ExpiresInSeconds)
            .Returns(3600);

        var command = new RefreshTokenCommand { RefreshToken = refreshToken };
        var handler = new RefreshTokenCommandHandler(_mockUow.Object, _mockPasswordHasher.Object, _mockJwtTokenService.Object, _mockLoggerRefresh.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("new_access_token");
        result.ExpiresIn.Should().Be(3600);
    }

    [Fact]
    public async Task RefreshTokenCommandHandler_WithExpiredRefreshToken_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var refreshToken = "expired_token";
        var hashedToken = "hashed_token";

        var refreshTokenRepoMock = new Mock<IRepository<RefreshToken>>();
        refreshTokenRepoMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Func<IQueryable<RefreshToken>, IQueryable<RefreshToken>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshToken)null);

        _mockUow
            .Setup(u => u.Repository<RefreshToken>())
            .Returns(refreshTokenRepoMock.Object);

        _mockPasswordHasher
            .Setup(p => p.Hash(refreshToken, string.Empty))
            .Returns(hashedToken);

        var command = new RefreshTokenCommand { RefreshToken = refreshToken };
        var handler = new RefreshTokenCommandHandler(_mockUow.Object, _mockPasswordHasher.Object, _mockJwtTokenService.Object, _mockLoggerRefresh.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task RefreshTokenCommandHandler_WithInactiveUser_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var refreshToken = "valid_refresh_token";
        var hashedToken = "hashed_token";

        var inactiveUser = new User
        {
            Id = userId,
            Email = "user@example.com",
            IsActive = false
        };

        var refreshTokenEntity = new RefreshToken
        {
            UserId = userId,
            Token = hashedToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };

        var refreshTokenRepoMock = new Mock<IRepository<RefreshToken>>();
        refreshTokenRepoMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Func<IQueryable<RefreshToken>, IQueryable<RefreshToken>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshTokenEntity);

        var userRepoMock = new Mock<IRepository<User>>();
        userRepoMock
            .Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveUser);

        _mockUow
            .Setup(u => u.Repository<RefreshToken>())
            .Returns(refreshTokenRepoMock.Object);

        _mockUow
            .Setup(u => u.Repository<User>())
            .Returns(userRepoMock.Object);

        _mockPasswordHasher
            .Setup(p => p.Hash(refreshToken, string.Empty))
            .Returns(hashedToken);

        var command = new RefreshTokenCommand { RefreshToken = refreshToken };
        var handler = new RefreshTokenCommandHandler(_mockUow.Object, _mockPasswordHasher.Object, _mockJwtTokenService.Object, _mockLoggerRefresh.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedException>(() => handler.Handle(command, CancellationToken.None));
    }

    #endregion
}
