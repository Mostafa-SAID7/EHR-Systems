using FluentAssertions;
using FluentValidation;
using EHRPlatform.Services.Clinical.Features.ClinicalNotes.Commands;
using EHRPlatform.Services.Clinical.Features.ClinicalNotes.Validation;
using Xunit;

namespace EHRPlatform.Tests.Unit.Application;

/// <summary>
/// Unit tests for Clinical validators.
/// Tests ICD-10 format, medical data constraints, and input validation.
/// HIPAA: Validators ensure data quality for protected health information.
/// </summary>
public class ClinicalNoteValidatorTests
{
    [Fact]
    public async Task CreateClinicalNoteCommandValidator_WithValidData_ShouldPass()
    {
        // Arrange
        var validator = new CreateClinicalNoteCommandValidator();
        var command = new CreateClinicalNoteCommand
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            EncounterDate = DateTime.UtcNow.AddDays(-1),
            EncounterType = "Office",
            Subjective = "Patient reports fatigue",
            Objective = "BP: 120/80",
            Assessment = "Possible anemia",
            Plan = "Order CBC"
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CreateClinicalNoteCommandValidator_WithoutPatientId_ShouldFail()
    {
        // Arrange
        var validator = new CreateClinicalNoteCommandValidator();
        var command = new CreateClinicalNoteCommand
        {
            PatientId = Guid.Empty,
            ProviderId = Guid.NewGuid(),
            EncounterDate = DateTime.UtcNow,
            EncounterType = "Office"
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "PatientId");
    }

    [Fact]
    public async Task CreateClinicalNoteCommandValidator_WithoutProviderId_ShouldFail()
    {
        // Arrange
        var validator = new CreateClinicalNoteCommandValidator();
        var command = new CreateClinicalNoteCommand
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.Empty,
            EncounterDate = DateTime.UtcNow,
            EncounterType = "Office"
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "ProviderId");
    }

    [Fact]
    public async Task CreateClinicalNoteCommandValidator_WithFutureEncounterDate_ShouldFail()
    {
        // Arrange
        var validator = new CreateClinicalNoteCommandValidator();
        var command = new CreateClinicalNoteCommand
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            EncounterDate = DateTime.UtcNow.AddDays(1), // Future date
            EncounterType = "Office"
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "EncounterDate");
    }

    [Theory]
    [InlineData("Office")]
    [InlineData("Telehealth")]
    [InlineData("Emergency")]
    [InlineData("Hospital")]
    public async Task CreateClinicalNoteCommandValidator_WithValidEncounterTypes_ShouldPass(string encounterType)
    {
        // Arrange
        var validator = new CreateClinicalNoteCommandValidator();
        var command = new CreateClinicalNoteCommand
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            EncounterDate = DateTime.UtcNow.AddDays(-1),
            EncounterType = encounterType
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task CreateClinicalNoteCommandValidator_WithInvalidEncounterType_ShouldFail()
    {
        // Arrange
        var validator = new CreateClinicalNoteCommandValidator();
        var command = new CreateClinicalNoteCommand
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            EncounterDate = DateTime.UtcNow.AddDays(-1),
            EncounterType = "InvalidType"
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "EncounterType");
    }

    [Fact]
    public async Task AddDiagnosisCommandValidator_WithValidICD10Code_ShouldPass()
    {
        // Arrange
        var validator = new AddDiagnosisCommandValidator();
        var command = new AddDiagnosisCommand
        {
            ClinicalNoteId = Guid.NewGuid(),
            DiagnosisCode = "I10",
            DiagnosisText = "Essential (primary) hypertension",
            DiagnosisType = "Principal"
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task AddDiagnosisCommandValidator_WithValidICD10CodeWithDecimal_ShouldPass()
    {
        // Arrange
        var validator = new AddDiagnosisCommandValidator();
        var command = new AddDiagnosisCommand
        {
            ClinicalNoteId = Guid.NewGuid(),
            DiagnosisCode = "I10.9",
            DiagnosisText = "Unspecified hypertension",
            DiagnosisType = "Secondary"
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("i10")] // lowercase
    [InlineData("10")]  // missing letter
    [InlineData("I")]   // letter only
    [InlineData("I100")] // wrong format
    [InlineData("A00.00")] // too many decimals
    public async Task AddDiagnosisCommandValidator_WithInvalidICD10Format_ShouldFail(string diagnosisCode)
    {
        // Arrange
        var validator = new AddDiagnosisCommandValidator();
        var command = new AddDiagnosisCommand
        {
            ClinicalNoteId = Guid.NewGuid(),
            DiagnosisCode = diagnosisCode,
            DiagnosisText = "Test diagnosis",
            DiagnosisType = "Principal"
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "DiagnosisCode");
    }

    [Theory]
    [InlineData("Principal")]
    [InlineData("Secondary")]
    public async Task AddDiagnosisCommandValidator_WithValidDiagnosisType_ShouldPass(string diagnosisType)
    {
        // Arrange
        var validator = new AddDiagnosisCommandValidator();
        var command = new AddDiagnosisCommand
        {
            ClinicalNoteId = Guid.NewGuid(),
            DiagnosisCode = "E11",
            DiagnosisText = "Type 2 diabetes mellitus",
            DiagnosisType = diagnosisType
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("primary")]
    [InlineData("secondary")]
    [InlineData("Other")]
    [InlineData("")]
    public async Task AddDiagnosisCommandValidator_WithInvalidDiagnosisType_ShouldFail(string diagnosisType)
    {
        // Arrange
        var validator = new AddDiagnosisCommandValidator();
        var command = new AddDiagnosisCommand
        {
            ClinicalNoteId = Guid.NewGuid(),
            DiagnosisCode = "E11",
            DiagnosisText = "Type 2 diabetes mellitus",
            DiagnosisType = diagnosisType
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "DiagnosisType");
    }

    [Fact]
    public async Task AddDiagnosisCommandValidator_WithoutClinicalNoteId_ShouldFail()
    {
        // Arrange
        var validator = new AddDiagnosisCommandValidator();
        var command = new AddDiagnosisCommand
        {
            ClinicalNoteId = Guid.Empty,
            DiagnosisCode = "I10",
            DiagnosisText = "Hypertension",
            DiagnosisType = "Principal"
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "ClinicalNoteId");
    }

    [Fact]
    public async Task RecordVitalsCommandValidator_WithValidRanges_ShouldPass()
    {
        // Arrange
        var validator = new RecordVitalsCommandValidator();
        var command = new RecordVitalsCommand
        {
            ClinicalNoteId = Guid.NewGuid(),
            Temperature = 98.6m,
            SystolicBP = 120,
            DiastolicBP = 80,
            HeartRate = 72,
            RespiratoryRate = 16
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task RecordVitalsCommandValidator_WithLowTemperature_ShouldFail()
    {
        // Arrange
        var validator = new RecordVitalsCommandValidator();
        var command = new RecordVitalsCommand
        {
            ClinicalNoteId = Guid.NewGuid(),
            Temperature = 94m, // Below normal range
            SystolicBP = 120,
            DiastolicBP = 80,
            HeartRate = 72,
            RespiratoryRate = 16
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Temperature");
    }

    [Fact]
    public async Task RecordVitalsCommandValidator_WithHighTemperature_ShouldFail()
    {
        // Arrange
        var validator = new RecordVitalsCommandValidator();
        var command = new RecordVitalsCommand
        {
            ClinicalNoteId = Guid.NewGuid(),
            Temperature = 107m, // Above normal range
            SystolicBP = 120,
            DiastolicBP = 80,
            HeartRate = 72,
            RespiratoryRate = 16
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.PropertyName == "Temperature");
    }

    [Fact]
    public async Task RecordVitalsCommandValidator_WithLowBloodPressure_ShouldFail()
    {
        // Arrange
        var validator = new RecordVitalsCommandValidator();
        var command = new RecordVitalsCommand
        {
            ClinicalNoteId = Guid.NewGuid(),
            Temperature = 98.6m,
            SystolicBP = 50, // Too low
            DiastolicBP = 40,
            HeartRate = 72,
            RespiratoryRate = 16
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task RecordVitalsCommandValidator_WithHighHeartRate_ShouldPass()
    {
        // Arrange
        var validator = new RecordVitalsCommandValidator();
        var command = new RecordVitalsCommand
        {
            ClinicalNoteId = Guid.NewGuid(),
            Temperature = 98.6m,
            SystolicBP = 120,
            DiastolicBP = 80,
            HeartRate = 150, // Elevated but valid
            RespiratoryRate = 16
        };

        // Act
        var result = await validator.ValidateAsync(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
