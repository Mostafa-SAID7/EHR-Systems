using EHRPlatform.Services.Identity.Domain.Entities;
using EHRPlatform.Tests.Common.Base;
using FluentAssertions;
using Xunit;

namespace EHRPlatform.Tests.Unit.Domain;

/// <summary>
/// Unit tests for User domain entity.
/// Tests core business logic: lockout, MFA, password validation.
/// </summary>
public class UserTests : UnitTestBase
{
    [Fact]
    public void Create_WithValidData_ShouldInitializeCorrectly()
    {
        // Arrange
        var email = "test@example.com";
        var firstName = "John";
        var lastName = "Doe";

        // Act
        var user = new User
        {
            Email = email,
            FirstName = firstName,
            LastName = lastName,
            IsActive = true,
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };

        // Assert
        user.Email.Should().Be(email);
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.IsActive.Should().BeTrue();
        user.FailedLoginAttempts.Should().Be(0);
        user.LockoutEnd.Should().BeNull();
        user.MfaEnabled.Should().BeFalse();
    }

    [Fact]
    public void IsLocked_WhenLockoutEndIsInFuture_ShouldReturnTrue()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            LockoutEnd = DateTime.UtcNow.AddMinutes(10)
        };

        // Act
        var isLocked = user.IsLocked();

        // Assert
        isLocked.Should().BeTrue();
    }

    [Fact]
    public void IsLocked_WhenLockoutEndIsInPast_ShouldReturnFalse()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            LockoutEnd = DateTime.UtcNow.AddMinutes(-10)
        };

        // Act
        var isLocked = user.IsLocked();

        // Assert
        isLocked.Should().BeFalse();
    }

    [Fact]
    public void IsLocked_WhenLockoutEndIsNull_ShouldReturnFalse()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            LockoutEnd = null
        };

        // Act
        var isLocked = user.IsLocked();

        // Assert
        isLocked.Should().BeFalse();
    }

    [Fact]
    public void Lock_ShouldSetLockoutEndTo15MinutesFromNow()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        var beforeLock = DateTime.UtcNow;

        // Act
        user.Lock();

        // Assert
        user.LockoutEnd.Should().HaveValue();
        user.LockoutEnd.Value.Should().BeGreaterThan(beforeLock.AddMinutes(14));
        user.LockoutEnd.Value.Should().BeLessThanOrEqualTo(DateTime.UtcNow.AddMinutes(16));
    }

    [Fact]
    public void Unlock_ShouldClearLockoutAndResetFailedAttempts()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            LockoutEnd = DateTime.UtcNow.AddMinutes(15),
            FailedLoginAttempts = 5
        };

        // Act
        user.Unlock();

        // Assert
        user.LockoutEnd.Should().BeNull();
        user.FailedLoginAttempts.Should().Be(0);
    }

    [Fact]
    public void MfaEnabled_ShouldBeToggleable()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            MfaEnabled = false
        };

        // Act
        user.MfaEnabled = true;

        // Assert
        user.MfaEnabled.Should().BeTrue();
    }

    [Fact]
    public void EmailConfirmed_ShouldBeToggleable()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            EmailConfirmed = false
        };

        // Act
        user.EmailConfirmed = true;

        // Assert
        user.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public void FailedLoginAttempts_ShouldIncrement()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt",
            FailedLoginAttempts = 0
        };

        // Act
        user.FailedLoginAttempts++;

        // Assert
        user.FailedLoginAttempts.Should().Be(1);
    }

    [Fact]
    public void LastLogin_ShouldBeUpdatable()
    {
        // Arrange
        var user = new User
        {
            Email = "test@example.com",
            PasswordHash = "hash",
            PasswordSalt = "salt"
        };
        var loginTime = DateTime.UtcNow;

        // Act
        user.LastLogin = loginTime;

        // Assert
        user.LastLogin.Should().Be(loginTime);
    }
}
