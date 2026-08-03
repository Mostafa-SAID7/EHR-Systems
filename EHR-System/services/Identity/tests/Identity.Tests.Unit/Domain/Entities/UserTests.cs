namespace Identity.Tests.Unit.Domain.Entities;

using FluentAssertions;
using Identity.Domain.Entities;
using Identity.Domain.Enums;
using Xunit;

/// <summary>
/// Unit tests for the User entity
/// </summary>
public sealed class UserTests
{
    [Fact]
    public void Create_WithValidParameters_ShouldCreateUser()
    {
        // Arrange
        var email = "test@example.com";
        var firstName = "John";
        var lastName = "Doe";
        var passwordHash = "hashedPassword123";

        // Act
        var user = User.Create(email, firstName, lastName, passwordHash);

        // Assert
        user.Email.Value.Should().Be(email);
        user.FirstName.Should().Be(firstName);
        user.LastName.Should().Be(lastName);
        user.Status.Should().Be(UserStatus.PendingEmailVerification);
        user.IsEmailVerified.Should().BeFalse();
        user.FailedLoginAttempts.Should().Be(0);
    }

    [Fact]
    public void RecordSuccessfulLogin_ShouldUpdateLoginStatus()
    {
        // Arrange
        var user = User.Create("test@example.com", "John", "Doe", "hash");

        // Act
        user.RecordSuccessfulLogin();

        // Assert
        user.LastLoginAt.Should().NotBeNull();
        user.FailedLoginAttempts.Should().Be(0);
    }

    [Fact]
    public void RecordFailedLoginAttempt_ShouldIncrementAttempts()
    {
        // Arrange
        var user = User.Create("test@example.com", "John", "Doe", "hash");

        // Act
        user.RecordFailedLoginAttempt();

        // Assert
        user.FailedLoginAttempts.Should().Be(1);
    }

    [Fact]
    public void RecordFailedLoginAttempt_WhenMaxAttemptsReached_ShouldLockAccount()
    {
        // Arrange
        var user = User.Create("test@example.com", "John", "Doe", "hash");
        const int maxAttempts = 5;

        // Act
        for (int i = 0; i < maxAttempts; i++)
        {
            user.RecordFailedLoginAttempt(maxAttempts);
        }

        // Assert
        user.Status.Should().Be(UserStatus.LockedOut);
        user.FailedLoginAttempts.Should().Be(maxAttempts);
    }

    [Fact]
    public void VerifyEmail_ShouldUpdateStatus()
    {
        // Arrange
        var user = User.Create("test@example.com", "John", "Doe", "hash");

        // Act
        user.VerifyEmail();

        // Assert
        user.IsEmailVerified.Should().BeTrue();
        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void ChangePassword_ShouldUpdatePasswordHash()
    {
        // Arrange
        var user = User.Create("test@example.com", "John", "Doe", "oldHash");
        var newHash = "newHash";

        // Act
        user.ChangePassword(newHash);

        // Assert
        user.PasswordHash.Hash.Should().Be(newHash);
    }

    [Fact]
    public void Suspend_ShouldChangeStatusToSuspended()
    {
        // Arrange
        var user = User.Create("test@example.com", "John", "Doe", "hash");
        user.VerifyEmail();

        // Act
        user.Suspend();

        // Assert
        user.Status.Should().Be(UserStatus.Suspended);
    }

    [Fact]
    public void Reactivate_WhenSuspended_ShouldChangeStatusToActive()
    {
        // Arrange
        var user = User.Create("test@example.com", "John", "Doe", "hash");
        user.VerifyEmail();
        user.Suspend();

        // Act
        user.Reactivate();

        // Assert
        user.Status.Should().Be(UserStatus.Active);
    }

    [Fact]
    public void AddRole_ShouldAddRoleToUser()
    {
        // Arrange
        var user = User.Create("test@example.com", "John", "Doe", "hash");
        var role = Role.Create("Admin", RoleType.Admin, "Administrator");

        // Act
        user.AddRole(role);

        // Assert
        user.Roles.Should().HaveCount(1);
        user.Roles.First().RoleId.Should().Be(role.Id);
    }

    [Fact]
    public void RemoveRole_ShouldRemoveRoleFromUser()
    {
        // Arrange
        var user = User.Create("test@example.com", "John", "Doe", "hash");
        var role = Role.Create("Admin", RoleType.Admin, "Administrator");
        user.AddRole(role);

        // Act
        user.RemoveRole(role.Id);

        // Assert
        user.Roles.Should().HaveCount(0);
    }
}
