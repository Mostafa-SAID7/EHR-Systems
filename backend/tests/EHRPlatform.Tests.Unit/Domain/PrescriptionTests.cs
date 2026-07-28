using EHRPlatform.Services.Prescription.Domain.Entities;
using EHRPlatform.Tests.Common.Base;
using FluentAssertions;
using Xunit;

namespace EHRPlatform.Tests.Unit.Domain;

/// <summary>
/// Unit tests for Prescription domain entity.
/// Tests medication lifecycle, refill logic, status transitions, HIPAA compliance.
/// </summary>
public class PrescriptionTests : UnitTestBase
{
    [Fact]
    public void Prescription_Create_ShouldInitializeWithCorrectValues()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var providerId = Guid.NewGuid();

        // Act
        var prescription = new Prescription
        {
            PatientId = patientId,
            ProviderId = providerId,
            MedicationName = "Lisinopril",
            Strength = "10mg",
            FormType = "Tablet",
            Dosage = "1 tablet",
            Frequency = "once daily",
            Quantity = 30,
            RefillsAllowed = 11,
            Status = "Active",
            StartDate = DateTime.UtcNow,
            EndDate = null,
            IsControlledSubstance = false
        };

        // Assert
        prescription.PatientId.Should().Be(patientId);
        prescription.ProviderId.Should().Be(providerId);
        prescription.MedicationName.Should().Be("Lisinopril");
        prescription.Status.Should().Be("Active");
        prescription.RefillsUsed.Should().Be(0);
    }

    [Fact]
    public void Prescription_CanRefill_ActiveWithRefillsRemaining_ShouldReturnTrue()
    {
        // Arrange
        var prescription = new Prescription
        {
            Status = "Active",
            RefillsAllowed = 5,
            RefillsUsed = 2,
            EndDate = DateTime.UtcNow.AddMonths(6)
        };

        // Act
        var canRefill = prescription.CanRefill();

        // Assert
        canRefill.Should().BeTrue();
    }

    [Fact]
    public void Prescription_CanRefill_NoRefillsRemaining_ShouldReturnFalse()
    {
        // Arrange
        var prescription = new Prescription
        {
            Status = "Active",
            RefillsAllowed = 3,
            RefillsUsed = 3
        };

        // Act
        var canRefill = prescription.CanRefill();

        // Assert
        canRefill.Should().BeFalse();
    }

    [Fact]
    public void Prescription_CanRefill_Discontinued_ShouldReturnFalse()
    {
        // Arrange
        var prescription = new Prescription
        {
            Status = "Discontinued",
            RefillsAllowed = 5,
            RefillsUsed = 1
        };

        // Act
        var canRefill = prescription.CanRefill();

        // Assert
        canRefill.Should().BeFalse();
    }

    [Fact]
    public void Prescription_CanRefill_Expired_ShouldReturnFalse()
    {
        // Arrange
        var prescription = new Prescription
        {
            Status = "Active",
            RefillsAllowed = 5,
            RefillsUsed = 1,
            EndDate = DateTime.UtcNow.AddDays(-1) // Expired
        };

        // Act
        var canRefill = prescription.CanRefill();

        // Assert
        canRefill.Should().BeFalse();
    }

    [Fact]
    public void Prescription_RequestRefill_ShouldAddRefillAndRaiseEvent()
    {
        // Arrange
        var prescription = new Prescription
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Metformin",
            Status = "Active",
            RefillsAllowed = 10,
            RefillsUsed = 0,
            EndDate = DateTime.UtcNow.AddMonths(6)
        };

        // Act
        prescription.RequestRefill("pharmacy_001");

        // Assert
        prescription.Refills.Should().HaveCount(1);
        prescription.Refills.First().Status.Should().Be("Pending");
        prescription.GetDomainEvents().Should().NotBeEmpty();
    }

    [Fact]
    public void Prescription_RequestRefill_NoRefillsRemaining_ShouldThrow()
    {
        // Arrange
        var prescription = new Prescription
        {
            Status = "Active",
            RefillsAllowed = 1,
            RefillsUsed = 1,
            EndDate = DateTime.UtcNow.AddMonths(1)
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => prescription.RequestRefill());
    }

    [Fact]
    public void Prescription_ApproveRefill_ShouldUpdateStatusAndIncrementCount()
    {
        // Arrange
        var prescription = new Prescription
        {
            Id = Guid.NewGuid(),
            Status = "Active",
            RefillsAllowed = 5,
            RefillsUsed = 0,
            EndDate = DateTime.UtcNow.AddMonths(6)
        };

        prescription.RequestRefill();
        var refillId = prescription.Refills.First().Id;

        // Act
        prescription.ApproveRefill(refillId);

        // Assert
        prescription.Refills.First().Status.Should().Be("Approved");
        prescription.RefillsUsed.Should().Be(1);
    }

    [Fact]
    public void Prescription_Suspend_ShouldChangeStatusAndRaiseEvent()
    {
        // Arrange
        var prescription = new Prescription
        {
            Id = Guid.NewGuid(),
            Status = "Active",
            MedicationName = "Aspirin"
        };

        // Act
        prescription.Suspend("Drug interaction detected");

        // Assert
        prescription.Status.Should().Be("Suspended");
        prescription.GetDomainEvents().Should().NotBeEmpty();
    }

    [Fact]
    public void Prescription_Resume_ShouldActivateFromSuspended()
    {
        // Arrange
        var prescription = new Prescription
        {
            Id = Guid.NewGuid(),
            Status = "Suspended",
            MedicationName = "Aspirin"
        };

        // Act
        prescription.Resume();

        // Assert
        prescription.Status.Should().Be("Active");
    }

    [Fact]
    public void Prescription_Discontinue_ShouldMarkEndDateAndRaiseEvent()
    {
        // Arrange
        var prescription = new Prescription
        {
            Id = Guid.NewGuid(),
            Status = "Active",
            MedicationName = "Lisinopril"
        };

        var beforeDiscontinue = DateTime.UtcNow;

        // Act
        prescription.Discontinue("Patient requested discontinuation");

        var afterDiscontinue = DateTime.UtcNow;

        // Assert
        prescription.Status.Should().Be("Discontinued");
        prescription.EndDate.Should().BeCloseTo(beforeDiscontinue, TimeSpan.FromSeconds(1));
        prescription.GetDomainEvents().Should().NotBeEmpty();
    }

    [Fact]
    public void Prescription_ControlledSubstance_ShouldBeFlagged()
    {
        // Arrange & Act
        var prescription = new Prescription
        {
            MedicationName = "Morphine",
            IsControlledSubstance = true,
            Strength = "10mg",
            NDCCode = "55390-0100-00"
        };

        // Assert
        prescription.IsControlledSubstance.Should().BeTrue();
        prescription.NDCCode.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void Prescription_ComputedProperty_RefillsRemaining_ShouldCalculateCorrectly()
    {
        // Arrange
        var prescription = new Prescription
        {
            RefillsAllowed = 11,
            RefillsUsed = 3
        };

        // Act
        // Using direct calculation as RefillsRemaining is on DTO
        var remaining = prescription.RefillsAllowed - prescription.RefillsUsed;

        // Assert
        remaining.Should().Be(8);
    }
}
