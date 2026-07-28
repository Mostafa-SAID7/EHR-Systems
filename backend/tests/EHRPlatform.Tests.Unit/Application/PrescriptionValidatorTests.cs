using EHRPlatform.Services.Prescription.Features.Prescriptions.Commands;
using EHRPlatform.Services.Prescription.Features.Prescriptions.Validation;
using EHRPlatform.Tests.Common.Base;
using FluentAssertions;
using Xunit;

namespace EHRPlatform.Tests.Unit.Application;

/// <summary>
/// Unit tests for Prescription service validators.
/// Tests medication safety: dosage validation, quantity checks, date constraints.
/// </summary>
public class PrescriptionValidatorTests : UnitTestBase
{
    private readonly IssuePrescriptionCommandValidator _validator = new();

    #region Valid Prescription Tests

    [Fact]
    public void IssuePrescriptionValidator_WithValidData_ShouldPass()
    {
        // Arrange
        var command = new IssuePrescriptionCommand
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Lisinopril",
            Strength = "10mg",
            Dosage = "1 tablet",
            Frequency = "once daily",
            Quantity = 30,
            RefillsAllowed = 11,
            StartDate = DateTime.UtcNow,
            EndDate = null
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void IssuePrescriptionValidator_WithAllRequiredFields_ShouldPass()
    {
        // Arrange
        var command = new IssuePrescriptionCommand
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Aspirin",
            Strength = "81mg",
            Dosage = "1 tablet",
            Frequency = "twice daily",
            Quantity = 60,
            RefillsAllowed = 5,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = null
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Patient and Provider Tests

    [Fact]
    public void IssuePrescriptionValidator_WithEmptyPatientId_ShouldFail()
    {
        // Arrange
        var command = new IssuePrescriptionCommand
        {
            PatientId = Guid.Empty,
            ProviderId = Guid.NewGuid(),
            MedicationName = "Metformin",
            Strength = "500mg",
            Dosage = "1 tablet",
            Frequency = "twice daily",
            Quantity = 60,
            RefillsAllowed = 0,
            StartDate = DateTime.UtcNow
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IssuePrescriptionValidator_WithEmptyProviderId_ShouldFail()
    {
        // Arrange
        var command = new IssuePrescriptionCommand
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.Empty,
            MedicationName = "Ibuprofen",
            Strength = "200mg",
            Dosage = "1 tablet",
            Frequency = "every 4-6 hours",
            Quantity = 30,
            RefillsAllowed = 2,
            StartDate = DateTime.UtcNow
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Medication Name Tests

    [Fact]
    public void IssuePrescriptionValidator_WithEmptyMedicationName_ShouldFail()
    {
        // Arrange
        var command = new IssuePrescriptionCommand
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = string.Empty,
            Strength = "10mg",
            Dosage = "1 tablet",
            Frequency = "once daily",
            Quantity = 30,
            RefillsAllowed = 0,
            StartDate = DateTime.UtcNow
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IssuePrescriptionValidator_WithMedicationNameExceeding255Chars_ShouldFail()
    {
        // Arrange
        var command = new IssuePrescriptionCommand
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = new string('A', 256),
            Strength = "10mg",
            Dosage = "1 tablet",
            Frequency = "once daily",
            Quantity = 30,
            RefillsAllowed = 0,
            StartDate = DateTime.UtcNow
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Strength and Dosage Tests

    [Fact]
    public void IssuePrescriptionValidator_WithEmptyStrength_ShouldFail()
    {
        // Arrange
        var command = new IssuePrescriptionCommand
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Atorvastatin",
            Strength = string.Empty,
            Dosage = "1 tablet",
            Frequency = "once daily",
            Quantity = 30,
            RefillsAllowed = 11,
            StartDate = DateTime.UtcNow
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IssuePrescriptionValidator_WithEmptyDosage_ShouldFail()
    {
        // Arrange
        var command = new IssuePrescriptionCommand
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Omeprazole",
            Strength = "20mg",
            Dosage = string.Empty,
            Frequency = "once daily",
            Quantity = 30,
            RefillsAllowed = 0,
            StartDate = DateTime.UtcNow
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IssuePrescriptionValidator_WithEmptyFrequency_ShouldFail()
    {
        // Arrange
        var command = new IssuePrescriptionCommand
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Sertraline",
            Strength = "50mg",
            Dosage = "1 tablet",
            Frequency = string.Empty,
            Quantity = 30,
            RefillsAllowed = 11,
            StartDate = DateTime.UtcNow
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    #endregion

    #region Quantity and Refills Tests

    [Fact]
    public void IssuePrescriptionValidator_WithZeroQuantity_ShouldFail()
    {
        // Arrange
        var command = new IssuePrescriptionCommand
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Amoxicillin",
            Strength = "500mg",
            Dosage = "1 capsule",
            Frequency = "three times daily",
            Quantity = 0,
            RefillsAllowed = 0,
            StartDate = DateTime.UtcNow
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IssuePrescriptionValidator_WithNegativeQuantity_ShouldFail()
    {
        // Arrange
        var command = new IssuePrescriptionCommand
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Ciprofloxacin",
            Strength = "500mg",
            Dosage = "1 tablet",
            Frequency = "twice daily",
            Quantity = -30,
            RefillsAllowed = 0,
            StartDate = DateTime.UtcNow
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IssuePrescriptionValidator_WithNegativeRefills_ShouldFail()
    {
        // Arrange
        var command = new IssuePrescriptionCommand
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Fluoxetine",
            Strength = "20mg",
            Dosage = "1 capsule",
            Frequency = "once daily",
            Quantity = 30,
            RefillsAllowed = -1,
            StartDate = DateTime.UtcNow
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IssuePrescriptionValidator_WithZeroRefills_ShouldPass()
    {
        // Arrange
        var command = new IssuePrescriptionCommand
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Azithromycin",
            Strength = "250mg",
            Dosage = "2 tablets",
            Frequency = "once daily",
            Quantity = 10,
            RefillsAllowed = 0,
            StartDate = DateTime.UtcNow
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion

    #region Date Tests

    [Fact]
    public void IssuePrescriptionValidator_WithFutureStartDate_ShouldFail()
    {
        // Arrange
        var command = new IssuePrescriptionCommand
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Vitamin D",
            Strength = "1000 IU",
            Dosage = "1 tablet",
            Frequency = "once daily",
            Quantity = 90,
            RefillsAllowed = 11,
            StartDate = DateTime.UtcNow.AddDays(1)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IssuePrescriptionValidator_WithEndDateBeforeStartDate_ShouldFail()
    {
        // Arrange
        var command = new IssuePrescriptionCommand
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Prednisone",
            Strength = "5mg",
            Dosage = "1 tablet",
            Frequency = "once daily",
            Quantity = 30,
            RefillsAllowed = 0,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void IssuePrescriptionValidator_WithValidEndDate_ShouldPass()
    {
        // Arrange
        var command = new IssuePrescriptionCommand
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Penicillin V",
            Strength = "250mg",
            Dosage = "1 tablet",
            Frequency = "four times daily",
            Quantity = 40,
            RefillsAllowed = 0,
            StartDate = DateTime.UtcNow.AddDays(-1),
            EndDate = DateTime.UtcNow.AddDays(9)
        };

        // Act
        var result = _validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    #endregion
}
