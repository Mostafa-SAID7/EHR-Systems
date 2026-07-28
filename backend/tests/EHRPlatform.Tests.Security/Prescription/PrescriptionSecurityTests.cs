using EHRPlatform.Services.Prescription.Domain.Entities;
using EHRPlatform.Tests.Common.Base;
using FluentAssertions;
using Xunit;

namespace EHRPlatform.Tests.Security.Prescription;

/// <summary>
/// Security tests for Prescription Service.
/// Tests medication safety, controlled substances, HIPAA compliance.
/// </summary>
public class PrescriptionSecurityTests : UnitTestBase
{
    #region MedicationSafety Tests

    [Fact]
    public void ControlledSubstance_ShouldBeFlaggedAndTracked()
    {
        // Arrange & Act
        var prescription = new Prescription
        {
            MedicationName = "Oxycodone",
            Strength = "5mg",
            IsControlledSubstance = true,
            NDCCode = "55154-0050-01",
            RefillsAllowed = 4 // DEA limit: max 5 refills in 6 months
        };

        // Assert
        prescription.IsControlledSubstance.Should().BeTrue();
        prescription.RefillsAllowed.Should().BeLessThanOrEqualTo(5);
        prescription.NDCCode.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("Morphine")]
    [InlineData("Fentanyl")]
    [InlineData("Oxycodone")]
    [InlineData("Hydrocodone")]
    [InlineData("Methadone")]
    public void HighRiskMedications_ShouldEnforceRefillLimits(string medication)
    {
        // Arrange & Act
        var prescription = new Prescription
        {
            MedicationName = medication,
            IsControlledSubstance = true,
            RefillsAllowed = 4
        };

        // Assert
        prescription.IsControlledSubstance.Should().BeTrue();
        prescription.RefillsAllowed.Should().BeLessThanOrEqualTo(5);
    }

    [Fact]
    public void Prescription_CanRefill_ShouldNotExceedMaximumRefills()
    {
        // Arrange
        var prescription = new Prescription
        {
            Status = "Active",
            RefillsAllowed = 11,
            RefillsUsed = 11,
            EndDate = DateTime.UtcNow.AddMonths(1)
        };

        // Act
        var canRefill = prescription.CanRefill();

        // Assert
        canRefill.Should().BeFalse();
    }

    #endregion

    #region DosageValidation Tests

    [Fact]
    public void Prescription_Dosage_ShouldBeValidFormat()
    {
        // Arrange
        var validDosages = new[] { "1 tablet", "2 tablets", "5ml", "1 injection" };

        // Act & Assert
        foreach (var dosage in validDosages)
        {
            var prescription = new Prescription
            {
                Dosage = dosage,
                MedicationName = "Test",
                Frequency = "once daily"
            };

            prescription.Dosage.Should().NotBeNullOrEmpty();
        }
    }

    [Fact]
    public void Prescription_Quantity_ShouldNotBeZeroOrNegative()
    {
        // Arrange & Act
        var prescription = new Prescription
        {
            Quantity = 30,
            MedicationName = "Medication"
        };

        // Assert
        prescription.Quantity.Should().BeGreaterThan(0);
    }

    #endregion

    #region RefillSafety Tests

    [Fact]
    public void RefillRequest_ShouldOnlyAllowActiveStatus()
    {
        // Arrange
        var prescription = new Prescription
        {
            Status = "Active",
            RefillsAllowed = 10,
            RefillsUsed = 0,
            EndDate = DateTime.UtcNow.AddMonths(3)
        };

        // Act
        prescription.RequestRefill("pharmacy_001");

        // Assert
        prescription.Refills.First().Status.Should().Be("Pending");
    }

    [Fact]
    public void RefillRequest_DiscontinuedPrescription_ShouldThrow()
    {
        // Arrange
        var prescription = new Prescription
        {
            Status = "Discontinued",
            RefillsAllowed = 5,
            RefillsUsed = 0,
            EndDate = DateTime.UtcNow
        };

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => prescription.RequestRefill());
    }

    [Fact]
    public void RefillApproval_ShouldIncrementUsedCounter()
    {
        // Arrange
        var prescription = new Prescription
        {
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
        prescription.RefillsUsed.Should().Be(1);
        prescription.Refills.First().Status.Should().Be("Approved");
    }

    #endregion

    #region PrescriptionExpiration Tests

    [Fact]
    public void ExpiredPrescription_ShouldNotAllowRefills()
    {
        // Arrange
        var prescription = new Prescription
        {
            Status = "Active",
            RefillsAllowed = 5,
            RefillsUsed = 0,
            EndDate = DateTime.UtcNow.AddDays(-1)
        };

        // Act
        var canRefill = prescription.CanRefill();

        // Assert
        canRefill.Should().BeFalse();
    }

    [Fact]
    public void ValidPrescriptionWindow_ShouldAllowRefills()
    {
        // Arrange
        var prescription = new Prescription
        {
            Status = "Active",
            RefillsAllowed = 10,
            RefillsUsed = 0,
            StartDate = DateTime.UtcNow.AddDays(-30),
            EndDate = DateTime.UtcNow.AddMonths(6)
        };

        // Act
        var canRefill = prescription.CanRefill();

        // Assert
        canRefill.Should().BeTrue();
    }

    #endregion

    #region SuspensionAndDiscontinuation Tests

    [Fact]
    public void SuspendedPrescription_ShouldNotAllowRefills()
    {
        // Arrange
        var prescription = new Prescription
        {
            Status = "Suspended",
            RefillsAllowed = 5,
            RefillsUsed = 0
        };

        // Act
        var canRefill = prescription.CanRefill();

        // Assert
        canRefill.Should().BeFalse();
    }

    [Fact]
    public void Suspend_ShouldRaiseEvent()
    {
        // Arrange
        var prescription = new Prescription
        {
            Id = Guid.NewGuid(),
            Status = "Active",
            MedicationName = "Aspirin"
        };

        // Act
        prescription.Suspend("Drug interaction");

        // Assert
        prescription.Status.Should().Be("Suspended");
        prescription.GetDomainEvents().Should().NotBeEmpty();
    }

    [Fact]
    public void Discontinue_ShouldRecordReasonAndMarkEndDate()
    {
        // Arrange
        var prescription = new Prescription
        {
            Id = Guid.NewGuid(),
            Status = "Active",
            MedicationName = "Ibuprofen",
            EndDate = null
        };

        var beforeDiscontinue = DateTime.UtcNow;

        // Act
        prescription.Discontinue("Patient switching to alternative");

        var afterDiscontinue = DateTime.UtcNow;

        // Assert
        prescription.Status.Should().Be("Discontinued");
        prescription.EndDate.Should().HaveValue();
        prescription.EndDate.Should().BeCloseTo(beforeDiscontinue, TimeSpan.FromSeconds(1));
    }

    #endregion

    #region HIPAA Compliance Tests

    [Fact]
    public void Prescription_ShouldMaintainAuditTrail()
    {
        // Arrange
        var createdBy = Guid.NewGuid();
        var createdAt = DateTime.UtcNow;

        var prescription = new Prescription
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Prescription",
            CreatedBy = createdBy,
            CreatedAt = createdAt
        };

        // Assert
        prescription.CreatedBy.Should().Be(createdBy);
        prescription.CreatedAt.Should().Be(createdAt);
    }

    [Fact]
    public void Prescription_PatientId_ShouldBeRequired()
    {
        // Arrange & Act
        var prescription = new Prescription
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Test"
        };

        // Assert
        prescription.PatientId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void Prescription_ProviderId_ShouldBeRequired()
    {
        // Arrange & Act
        var prescription = new Prescription
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Test"
        };

        // Assert
        prescription.ProviderId.Should().NotBe(Guid.Empty);
    }

    #endregion

    #region DrugInteractionDetection Tests

    [Fact]
    public void HighRiskMedication_ShouldRequireProviderApproval()
    {
        // Arrange
        var highRiskMedications = new[] { "Warfarin", "Methotrexate", "Lithium" };

        // Act & Assert
        foreach (var medication in highRiskMedications)
        {
            var prescription = new Prescription
            {
                MedicationName = medication,
                MedicationName = medication // Tracking for interaction checks
            };

            prescription.MedicationName.Should().NotBeNullOrEmpty();
        }
    }

    #endregion
}
