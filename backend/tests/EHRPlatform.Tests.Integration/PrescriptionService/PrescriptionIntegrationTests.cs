#nullable enable

using System;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using EHRPlatform.Services.Prescription.Domain.Entities;
using EHRPlatform.Tests.Common.Base;
using Microsoft.EntityFrameworkCore;

namespace EHRPlatform.Tests.Integration.PrescriptionService;

/// <summary>
/// Integration tests for Prescription Service with real PostgreSQL.
/// Tests prescription lifecycle, refills, medication safety, and pharmacy workflows.
/// </summary>
public class PrescriptionIntegrationTests : IntegrationTestBase
{
    [Fact]
    public async Task CreatePrescription_WithValidData_ShouldPersist()
    {
        // Arrange
        var prescription = new Prescription
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Lisinopril",
            Strength = "10mg",
            FormType = "Tablet",
            Dosage = "1 tablet",
            Frequency = "once daily",
            Quantity = 30,
            RefillsAllowed = 11,
            Status = "Active",
            StartDate = DateTime.UtcNow,
            CreatedBy = Guid.Empty
        };

        // Act
        DbContext.Set<Prescription>().Add(prescription);
        await SaveChangesAsync();

        // Assert
        var retrieved = await DbContext.Set<Prescription>().FindAsync(prescription.Id);
        retrieved.Should().NotBeNull();
        retrieved!.MedicationName.Should().Be("Lisinopril");
    }

    [Fact]
    public async Task RequestRefill_ShouldCreatePendingRefillRequest()
    {
        // Arrange
        var prescription = new Prescription
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Metformin",
            Status = "Active",
            Quantity = 60,
            RefillsAllowed = 10,
            RefillsUsed = 0,
            EndDate = DateTime.UtcNow.AddMonths(6),
            CreatedBy = Guid.Empty
        };

        DbContext.Set<Prescription>().Add(prescription);
        await SaveChangesAsync();

        // Act
        prescription.RequestRefill("pharmacy_001");
        DbContext.Set<Prescription>().Update(prescription);
        await SaveChangesAsync();

        // Assert
        var refill = prescription.Refills.First();
        refill.Status.Should().Be("Pending");
        refill.PharmacyId.Should().Be("pharmacy_001");
    }

    [Fact]
    public async Task ApproveRefill_ShouldUpdateStatusAndIncrementUsedCount()
    {
        // Arrange
        var prescription = new Prescription
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Aspirin",
            Status = "Active",
            Quantity = 100,
            RefillsAllowed = 5,
            RefillsUsed = 0,
            EndDate = DateTime.UtcNow.AddMonths(12),
            CreatedBy = Guid.Empty
        };

        DbContext.Set<Prescription>().Add(prescription);
        await SaveChangesAsync();

        prescription.RequestRefill();
        var refillId = prescription.Refills.First().Id;
        DbContext.Set<Prescription>().Update(prescription);
        await SaveChangesAsync();

        // Act
        prescription.ApproveRefill(refillId);
        DbContext.Set<Prescription>().Update(prescription);
        await SaveChangesAsync();

        // Assert
        var retrieved = await DbContext.Set<Prescription>().FindAsync(prescription.Id);
        retrieved!.RefillsUsed.Should().Be(1);
        retrieved.Refills.First().Status.Should().Be("Approved");
    }

    [Fact]
    public async Task SuspendPrescription_ShouldPreventRefills()
    {
        // Arrange
        var prescription = new Prescription
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Ibuprofen",
            Status = "Active",
            Quantity = 30,
            RefillsAllowed = 2,
            EndDate = DateTime.UtcNow.AddMonths(3),
            CreatedBy = Guid.Empty
        };

        DbContext.Set<Prescription>().Add(prescription);
        await SaveChangesAsync();

        // Act
        prescription.Suspend("Drug interaction");
        DbContext.Set<Prescription>().Update(prescription);
        await SaveChangesAsync();

        // Assert
        var retrieved = await DbContext.Set<Prescription>().FindAsync(prescription.Id);
        retrieved!.Status.Should().Be("Suspended");
        retrieved.CanRefill().Should().BeFalse();
    }

    [Fact]
    public async Task ResumePrescription_ShouldRestoreRefillCapability()
    {
        // Arrange
        var prescription = new Prescription
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Omeprazole",
            Status = "Suspended",
            Quantity = 30,
            RefillsAllowed = 11,
            RefillsUsed = 0,
            EndDate = DateTime.UtcNow.AddMonths(6),
            CreatedBy = Guid.Empty
        };

        DbContext.Set<Prescription>().Add(prescription);
        await SaveChangesAsync();

        // Act
        prescription.Resume();
        DbContext.Set<Prescription>().Update(prescription);
        await SaveChangesAsync();

        // Assert
        var retrieved = await DbContext.Set<Prescription>().FindAsync(prescription.Id);
        retrieved!.Status.Should().Be("Active");
        retrieved.CanRefill().Should().BeTrue();
    }

    [Fact]
    public async Task DiscontinuePrescription_ShouldMarkEndDate()
    {
        // Arrange
        var prescription = new Prescription
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Sertraline",
            Status = "Active",
            Quantity = 30,
            RefillsAllowed = 11,
            EndDate = null,
            CreatedBy = Guid.Empty
        };

        DbContext.Set<Prescription>().Add(prescription);
        await SaveChangesAsync();

        // Act
        prescription.Discontinue("Patient switching medications");
        DbContext.Set<Prescription>().Update(prescription);
        await SaveChangesAsync();

        // Assert
        var retrieved = await DbContext.Set<Prescription>().FindAsync(prescription.Id);
        retrieved!.Status.Should().Be("Discontinued");
        retrieved.EndDate.Should().HaveValue();
    }

    [Fact]
    public async Task QueryPrescriptionsByPatient_ShouldReturnOnlyActive()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        
        var activePrescription = new Prescription
        {
            PatientId = patientId,
            ProviderId = Guid.NewGuid(),
            MedicationName = "Active Med",
            Status = "Active",
            Quantity = 30,
            RefillsAllowed = 11,
            EndDate = DateTime.UtcNow.AddMonths(3),
            CreatedBy = Guid.Empty
        };

        var discontinuedPrescription = new Prescription
        {
            PatientId = patientId,
            ProviderId = Guid.NewGuid(),
            MedicationName = "Discontinued Med",
            Status = "Discontinued",
            Quantity = 30,
            RefillsAllowed = 5,
            EndDate = DateTime.UtcNow.AddDays(-1),
            CreatedBy = Guid.Empty
        };

        DbContext.Set<Prescription>().AddRange(activePrescription, discontinuedPrescription);
        await SaveChangesAsync();

        // Act
        var activePrescriptions = await DbContext.Set<Prescription>()
            .Where(p => p.PatientId == patientId && p.Status == "Active")
            .ToListAsync();

        // Assert
        activePrescriptions.Should().HaveCount(1);
        activePrescriptions.First().MedicationName.Should().Be("Active Med");
    }

    [Fact]
    public async Task ControlledSubstancePrescription_ShouldBeTracked()
    {
        // Arrange
        var prescription = new Prescription
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Morphine",
            Strength = "10mg",
            IsControlledSubstance = true,
            NDCCode = "12345-0001-01",
            Status = "Active",
            Quantity = 30,
            RefillsAllowed = 5,
            CreatedBy = Guid.Empty
        };

        DbContext.Set<Prescription>().Add(prescription);
        await SaveChangesAsync();

        // Act
        var retrieved = await DbContext.Set<Prescription>().FirstOrDefaultAsync(p => p.Id == prescription.Id);

        // Assert
        retrieved!.IsControlledSubstance.Should().BeTrue();
        retrieved.NDCCode.Should().Be("12345-0001-01");
    }

    [Fact]
    public async Task MultipleRefillRequests_ShouldTrackAll()
    {
        // Arrange
        var prescription = new Prescription
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Atorvastatin",
            Status = "Active",
            Quantity = 30,
            RefillsAllowed = 11,
            RefillsUsed = 0,
            EndDate = DateTime.UtcNow.AddMonths(12),
            CreatedBy = Guid.Empty
        };

        DbContext.Set<Prescription>().Add(prescription);
        await SaveChangesAsync();

        // Act - Request multiple refills
        prescription.RequestRefill("pharmacy_001");
        DbContext.Set<Prescription>().Update(prescription);
        await SaveChangesAsync();

        prescription.RequestRefill("pharmacy_002");
        DbContext.Set<Prescription>().Update(prescription);
        await SaveChangesAsync();

        // Assert
        var retrieved = await DbContext.Set<Prescription>().FindAsync(prescription.Id);
        retrieved!.Refills.Should().HaveCount(2);
        retrieved.Refills.Should().AllSatisfy(r => r.Status.Should().Be("Pending"));
    }

    [Fact]
    public async Task RefillNotAllowedWhenExpired_ShouldReturnFalse()
    {
        // Arrange
        var prescription = new Prescription
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Amoxicillin",
            Status = "Active",
            Quantity = 30,
            RefillsAllowed = 2,
            RefillsUsed = 0,
            EndDate = DateTime.UtcNow.AddDays(-1), // Expired
            CreatedBy = Guid.Empty
        };

        DbContext.Set<Prescription>().Add(prescription);
        await SaveChangesAsync();

        // Act & Assert
        prescription.CanRefill().Should().BeFalse();
    }
}
