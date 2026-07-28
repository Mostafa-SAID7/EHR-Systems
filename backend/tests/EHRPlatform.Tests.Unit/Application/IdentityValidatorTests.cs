using EHRPlatform.Services.Identity.Features.Auth.Commands;
using EHRPlatform.Services.Identity.Features.Auth.Validation;
using EHRPlatform.Services.Identity.Features.Users.Commands;
using EHRPlatform.Services.Identity.Features.Users.Validation;
using EHRPlatform.Tests.Common.Base;
using FluentAssertions;
using FluentValidation;
using Xunit;

namespace EHRPlatform.Tests.Unit.Application;

/// <summary>
/// Unit tests for Identity service validators.
/// Tests all validation rules: email, password strength, field length, role validation.
/// </summary>
public class IdentityValidatorTests : UnitTestBase
{
    private readonly LoginCommandValidator _loginValidator = new();
    private readonly RegisterCommandValidator _registerValidator = new();
    private readonly CreateUserCommandValidator _createUserValidator = new();

    #region LoginCommandValidator Tests

    [Fact]
    public void LoginCommandValidator_WithValidCredentials_ShouldPass()
    {
        // Arrange
        var command = new LoginCommand { Email = "user@example.com", Password = "Password123!" };

        // Act
        var result = _loginValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void LoginCommandValidator_WithEmptyEmail_ShouldFail()
    {
        // Arrange
        var command = new LoginCommand { Email = string.Empty, Password = "Password123!" };

        // Act
        var result = _loginValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.Email));
    }

    [Fact]
    public void LoginCommandValidator_WithInvalidEmailFormat_ShouldFail()
    {
        // Arrange
        var command = new LoginCommand { Email = "not-an-email", Password = "Password123!" };

        // Act
        var result = _loginValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.Email));
    }

    [Fact]
    public void LoginCommandValidator_WithEmptyPassword_ShouldFail()
    {
        // Arrange
        var command = new LoginCommand { Email = "user@example.com", Password = string.Empty };

        // Act
        var result = _loginValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == nameof(command.Password));
    }

    [Fact]
    public void LoginCommandValidator_WithPasswordLessThan8Chars_ShouldFail()
    {
        // Arrange
        var command = new LoginCommand { Email = "user@example.com", Password = "Pass1!" };

        // Act
        var result = _loginValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Password));
    }

    #endregion

    #region RegisterCommandValidator Tests

    [Fact]
    public void RegisterCommandValidator_WithValidData_ShouldPass()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "SecurePass123!"
        };

        // Act
        var result = _registerValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RegisterCommandValidator_WithPasswordLessThan12Chars_ShouldFail()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "Pass1!"
        };

        // Act
        var result = _registerValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Password));
    }

    [Fact]
    public void RegisterCommandValidator_WithPasswordMissingUppercase_ShouldFail()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "securepass123!"
        };

        // Act
        var result = _registerValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Password));
    }

    [Fact]
    public void RegisterCommandValidator_WithPasswordMissingLowercase_ShouldFail()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "SECUREPASS123!"
        };

        // Act
        var result = _registerValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Password));
    }

    [Fact]
    public void RegisterCommandValidator_WithPasswordMissingDigit_ShouldFail()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "SecurePass!"
        };

        // Act
        var result = _registerValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Password));
    }

    [Fact]
    public void RegisterCommandValidator_WithPasswordMissingSpecialChar_ShouldFail()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe",
            Password = "SecurePass123"
        };

        // Act
        var result = _registerValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Password));
    }

    [Fact]
    public void RegisterCommandValidator_WithFirstNameExceeding100Chars_ShouldFail()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "user@example.com",
            FirstName = new string('A', 101),
            LastName = "Doe",
            Password = "SecurePass123!"
        };

        // Act
        var result = _registerValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.FirstName));
    }

    [Fact]
    public void RegisterCommandValidator_WithEmptyFirstName_ShouldFail()
    {
        // Arrange
        var command = new RegisterCommand
        {
            Email = "user@example.com",
            FirstName = string.Empty,
            LastName = "Doe",
            Password = "SecurePass123!"
        };

        // Act
        var result = _registerValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.FirstName));
    }

    #endregion

    #region CreateUserCommandValidator Tests

    [Fact]
    public void CreateUserCommandValidator_WithValidData_ShouldPass()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe",
            Role = "Doctor",
            CreatedBy = Guid.NewGuid()
        };

        // Act
        var result = _createUserValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateUserCommandValidator_WithInvalidRole_ShouldFail()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe",
            Role = "InvalidRole",
            CreatedBy = Guid.NewGuid()
        };

        // Act
        var result = _createUserValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Role));
    }

    [Fact]
    public void CreateUserCommandValidator_WithValidRoles_ShouldPass(string role)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe",
            Role = role,
            CreatedBy = Guid.NewGuid()
        };

        // Act
        var result = _createUserValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("Admin")]
    [InlineData("Doctor")]
    [InlineData("Nurse")]
    [InlineData("Receptionist")]
    [InlineData("Patient")]
    public void CreateUserCommandValidator_WithEachValidRole_ShouldPass(string role)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe",
            Role = role,
            CreatedBy = Guid.NewGuid()
        };

        // Act
        var result = _createUserValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void CreateUserCommandValidator_WithEmptyCreatedBy_ShouldFail()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "user@example.com",
            FirstName = "John",
            LastName = "Doe",
            Role = "Doctor",
            CreatedBy = Guid.Empty
        };

        // Act
        var result = _createUserValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.CreatedBy));
    }

    #endregion
}
