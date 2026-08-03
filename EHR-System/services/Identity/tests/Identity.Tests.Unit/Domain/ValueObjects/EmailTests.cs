namespace Identity.Tests.Unit.Domain.ValueObjects;

using FluentAssertions;
using Identity.Domain.Exceptions;
using Identity.Domain.ValueObjects;
using Xunit;

/// <summary>
/// Unit tests for the Email value object
/// </summary>
public sealed class EmailTests
{
    [Fact]
    public void Create_WithValidEmail_ShouldSucceed()
    {
        // Arrange
        var validEmail = "user@example.com";

        // Act
        var email = new Email(validEmail);

        // Assert
        email.Value.Should().Be(validEmail);
    }

    [Fact]
    public void Create_WithInvalidEmail_ShouldThrowInvalidEmailException()
    {
        // Arrange
        var invalidEmail = "not-an-email";

        // Act & Assert
        var action = () => new Email(invalidEmail);
        action.Should().Throw<InvalidEmailException>();
    }

    [Fact]
    public void Create_WithEmptyEmail_ShouldThrowInvalidEmailException()
    {
        // Arrange
        var emptyEmail = string.Empty;

        // Act & Assert
        var action = () => new Email(emptyEmail);
        action.Should().Throw<InvalidEmailException>();
    }

    [Fact]
    public void Create_WithNullEmail_ShouldThrowInvalidEmailException()
    {
        // Arrange
        string? nullEmail = null;

        // Act & Assert
        var action = () => new Email(nullEmail!);
        action.Should().Throw<InvalidEmailException>();
    }

    [Fact]
    public void Equality_WithSameValue_ShouldBeEqual()
    {
        // Arrange
        var email1 = new Email("test@example.com");
        var email2 = new Email("test@example.com");

        // Act & Assert
        email1.Should().Be(email2);
    }

    [Fact]
    public void Equality_WithDifferentValues_ShouldNotBeEqual()
    {
        // Arrange
        var email1 = new Email("test1@example.com");
        var email2 = new Email("test2@example.com");

        // Act & Assert
        email1.Should().NotBe(email2);
    }

    [Fact]
    public void ImplicitConversion_ToString_ShouldReturnEmailValue()
    {
        // Arrange
        var email = new Email("test@example.com");

        // Act
        string result = email;

        // Assert
        result.Should().Be("test@example.com");
    }
}
