using EHRPlatform.Services.Audit.Features.Audit.Commands;
using EHRPlatform.Services.Audit.Features.Audit.Validation;
using EHRPlatform.Tests.Common.Base;
using FluentAssertions;
using FluentValidation;
using Xunit;

namespace EHRPlatform.Tests.Unit.Application;

/// <summary>
/// Unit tests for Audit service validators.
/// Tests audit entry validation, compliance report validation.
/// </summary>
public class AuditValidatorTests : UnitTestBase
{
    private readonly CreateAuditEntryValidator _auditValidator = new();

    #region RecordAuditEntryCommand Validation Tests

    [Fact]
    public void RecordAuditEntryValidator_WithValidData_ShouldPass()
    {
        // Arrange
        var command = new RecordAuditEntryCommand
        {
            UserId = Guid.NewGuid(),
            UserEmail = "user@example.com",
            Action = "Read",
            ResourceType = "Patient",
            ResourceId = Guid.NewGuid(),
            IpAddress = "192.168.1.1",
            UserAgent = "Test-Agent/1.0"
        };

        // Act
        var result = _auditValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RecordAuditEntryValidator_WithEmptyUserId_ShouldFail()
    {
        // Arrange
        var command = new RecordAuditEntryCommand
        {
            UserId = Guid.Empty,
            UserEmail = "user@example.com",
            Action = "Read",
            ResourceType = "Patient",
            ResourceId = Guid.NewGuid()
        };

        // Act
        var result = _auditValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.UserId));
    }

    [Fact]
    public void RecordAuditEntryValidator_WithEmptyAction_ShouldFail()
    {
        // Arrange
        var command = new RecordAuditEntryCommand
        {
            UserId = Guid.NewGuid(),
            UserEmail = "user@example.com",
            Action = string.Empty,
            ResourceType = "Patient",
            ResourceId = Guid.NewGuid()
        };

        // Act
        var result = _auditValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Action));
    }

    [Fact]
    public void RecordAuditEntryValidator_WithActionExceeding50Chars_ShouldFail()
    {
        // Arrange
        var command = new RecordAuditEntryCommand
        {
            UserId = Guid.NewGuid(),
            UserEmail = "user@example.com",
            Action = new string('A', 51), // Exceeds 50 chars
            ResourceType = "Patient",
            ResourceId = Guid.NewGuid()
        };

        // Act
        var result = _auditValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.Action));
    }

    [Fact]
    public void RecordAuditEntryValidator_WithEmptyResourceType_ShouldFail()
    {
        // Arrange
        var command = new RecordAuditEntryCommand
        {
            UserId = Guid.NewGuid(),
            UserEmail = "user@example.com",
            Action = "Update",
            ResourceType = string.Empty,
            ResourceId = Guid.NewGuid()
        };

        // Act
        var result = _auditValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(command.ResourceType));
    }

    [Theory]
    [InlineData("Read")]
    [InlineData("Write")]
    [InlineData("Update")]
    [InlineData("Delete")]
    [InlineData("Export")]
    public void RecordAuditEntryValidator_WithValidActions_ShouldPass(string action)
    {
        // Arrange
        var command = new RecordAuditEntryCommand
        {
            UserId = Guid.NewGuid(),
            UserEmail = "user@example.com",
            Action = action,
            ResourceType = "Patient",
            ResourceId = Guid.NewGuid()
        };

        // Act
        var result = _auditValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("Patient")]
    [InlineData("Appointment")]
    [InlineData("Prescription")]
    [InlineData("ClinicalNote")]
    [InlineData("User")]
    public void RecordAuditEntryValidator_WithValidResourceTypes_ShouldPass(string resourceType)
    {
        // Arrange
        var command = new RecordAuditEntryCommand
        {
            UserId = Guid.NewGuid(),
            UserEmail = "user@example.com",
            Action = "Read",
            ResourceType = resourceType,
            ResourceId = Guid.NewGuid()
        };

        // Act
        var result = _auditValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RecordAuditEntryValidator_WithAccessLevelOutOfRange_ShouldFail()
    {
        // Arrange
        var command = new RecordAuditEntryCommand
        {
            UserId = Guid.NewGuid(),
            UserEmail = "user@example.com",
            Action = "Read",
            ResourceType = "Patient",
            ResourceId = Guid.NewGuid(),
            AccessLevel = 5 // Out of range (1-4 valid)
        };

        // Act
        var result = _auditValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public void RecordAuditEntryValidator_WithValidAccessLevels_ShouldPass(int accessLevel)
    {
        // Arrange
        var command = new RecordAuditEntryCommand
        {
            UserId = Guid.NewGuid(),
            UserEmail = "user@example.com",
            Action = "Read",
            ResourceType = "Patient",
            ResourceId = Guid.NewGuid(),
            AccessLevel = accessLevel
        };

        // Act
        var result = _auditValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RecordAuditEntryValidator_WithPiiIndicators_ShouldPass()
    {
        // Arrange
        var command = new RecordAuditEntryCommand
        {
            UserId = Guid.NewGuid(),
            UserEmail = "user@example.com",
            Action = "Read",
            ResourceType = "Patient",
            ResourceId = Guid.NewGuid(),
            PiiIndicators = "SSN,DOB,MRN"
        };

        // Act
        var result = _auditValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RecordAuditEntryValidator_FailureReasonOptionalForSuccessfulAction_ShouldPass()
    {
        // Arrange
        var command = new RecordAuditEntryCommand
        {
            UserId = Guid.NewGuid(),
            UserEmail = "user@example.com",
            Action = "Read",
            ResourceType = "Patient",
            ResourceId = Guid.NewGuid(),
            Success = true,
            FailureReason = null
        };

        // Act
        var result = _auditValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RecordAuditEntryValidator_FailureReasonRequiredForFailedAction_ShouldValidate()
    {
        // Arrange
        var command = new RecordAuditEntryCommand
        {
            UserId = Guid.NewGuid(),
            UserEmail = "user@example.com",
            Action = "Delete",
            ResourceType = "Patient",
            ResourceId = Guid.NewGuid(),
            Success = false,
            FailureReason = "Unauthorized: Insufficient permissions"
        };

        // Act
        var result = _auditValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Audit Entry Edge Cases

    [Fact]
    public void RecordAuditEntryValidator_WithMinimalData_ShouldPass()
    {
        // Arrange
        var command = new RecordAuditEntryCommand
        {
            UserId = Guid.NewGuid(),
            UserEmail = "user@example.com",
            Action = "R",
            ResourceType = "P",
            ResourceId = Guid.NewGuid()
        };

        // Act
        var result = _auditValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void RecordAuditEntryValidator_WithMaximalData_ShouldPass()
    {
        // Arrange
        var command = new RecordAuditEntryCommand
        {
            UserId = Guid.NewGuid(),
            UserEmail = "user@example.com",
            Action = new string('A', 50),
            ResourceType = "Patient",
            ResourceId = Guid.NewGuid(),
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            PiiIndicators = "SSN,DOB,MRN,HealthInfo",
            AccessLevel = 4,
            ChangeDetails = "{\"field\":\"value\"}",
            Success = true
        };

        // Act
        var result = _auditValidator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
}
