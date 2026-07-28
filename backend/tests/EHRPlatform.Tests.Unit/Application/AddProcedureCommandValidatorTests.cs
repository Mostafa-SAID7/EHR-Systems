using FluentAssertions;
using FluentValidation;
using EHRPlatform.Services.Clinical.Features.ClinicalNotes.Commands;
using EHRPlatform.Services.Clinical.Features.ClinicalNotes.Validation;
using Xunit;

namespace EHRPlatform.Tests.Unit.Application;

/// <summary>
/// Unit tests for AddProcedureCommand validator.
/// Tests procedure code validation and CPT/HCPCS format enforcement.
/// HIPAA: Procedure codes must be accurate for billing and compliance.
/// </summary>
public class AddProcedureCommandValidatorTests
{
    [Fact]
    public async Task AddProcedureCommandValidator_WithValidData_ShouldPass()
    {
        // Arrange
        var validator = new AddProcedureCommandValidator();
        var command = new AddProcedureCommand
        {
            ClinicalNoteId = Guid.NewGuid(),
            ProcedureName = "Chest X-Ray",
            ProcedureCode = "71046",
            Result = "Normal, no acute findings"
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task AddProcedureCommandValidator_WithoutClinicalNoteId_ShouldFail()
    {
        // Arrange
        var validator = new AddProcedureCommandValidator();
        var command = new AddProcedureCommand
        {
            ClinicalNoteId = Guid.Empty,
            ProcedureName = "Chest X-Ray",
            ProcedureCode = "71046"
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "ClinicalNoteId");
    }

    [Fact]
    public async Task AddProcedureCommandValidator_WithoutProcedureName_ShouldFail()
    {
        // Arrange
        var validator = new AddProcedureCommandValidator();
        var command = new AddProcedureCommand
        {
            ClinicalNoteId = Guid.NewGuid(),
            ProcedureName = "",
            ProcedureCode = "71046"
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task AddProcedureCommandValidator_WithoutProcedureCode_ShouldFail()
    {
        // Arrange
        var validator = new AddProcedureCommandValidator();
        var command = new AddProcedureCommand
        {
            ClinicalNoteId = Guid.NewGuid(),
            ProcedureName = "Chest X-Ray",
            ProcedureCode = ""
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("71046")]   // Chest X-Ray
    [InlineData("93000")]   // ECG
    [InlineData("80053")]   // Comprehensive metabolic panel
    [InlineData("99213")]   // Office visit
    public async Task AddProcedureCommandValidator_WithValidCPTCodes_ShouldPass(string procedureCode)
    {
        // Arrange
        var validator = new AddProcedureCommandValidator();
        var command = new AddProcedureCommand
        {
            ClinicalNoteId = Guid.NewGuid(),
            ProcedureName = "Test Procedure",
            ProcedureCode = procedureCode
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task AddProcedureCommandValidator_WithLongProcedureName_ShouldPass()
    {
        // Arrange
        var validator = new AddProcedureCommandValidator();
        var command = new AddProcedureCommand
        {
            ClinicalNoteId = Guid.NewGuid(),
            ProcedureName = "Comprehensive metabolic panel with liver function tests and renal function assessment",
            ProcedureCode = "80053"
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
