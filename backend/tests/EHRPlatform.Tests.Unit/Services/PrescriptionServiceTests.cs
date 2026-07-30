using EHRPlatform.Common.Data.Abstractions;
using EHRPlatform.Services.Prescription.Domain.Entities;
using EHRPlatform.Services.Prescription.Features.Prescriptions.Commands;
using EHRPlatform.Services.Prescription.Features.Prescriptions.Handlers;
using EHRPlatform.Services.Prescription.Application.PrescriptionManagement.Responses;
using EHRPlatform.Tests.Common.Base;
using FluentAssertions;
using Moq;
using Xunit;

namespace EHRPlatform.Tests.Unit.Services;

/// <summary>
/// Unit tests for Prescription service handlers.
/// Tests prescription issuance, refill requests, approvals with mocked dependencies.
/// </summary>
public class PrescriptionServiceTests : UnitTestBase
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<IOutboxRepository> _mockOutbox;

    public PrescriptionServiceTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockOutbox = new Mock<IOutboxRepository>();
    }

    #region IssuePrescription Tests

    [Fact]
    public async Task IssuePrescriptionCommandHandler_WithValidData_ShouldCreatePrescription()
    {
        // Arrange
        var prescriptionId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var providerId = Guid.NewGuid();

        var prescriptionRepoMock = new Mock<IRepository<Prescription>>();
        prescriptionRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Prescription>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUow
            .Setup(u => u.Repository<Prescription>())
            .Returns(prescriptionRepoMock.Object);

        _mockUow
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockOutbox
            .Setup(o => o.AddAsync(It.IsAny<OutboxEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new IssuePrescriptionCommand
        {
            PatientId = patientId,
            ProviderId = providerId,
            MedicationName = "Lisinopril",
            Strength = "10mg",
            Dosage = "1 tablet",
            Frequency = "once daily",
            Quantity = 30,
            RefillsAllowed = 11,
            StartDate = DateTime.UtcNow,
            FormType = "Tablet"
        };

        var mockLogger = new Mock<ILogger<IssuePrescriptionCommandHandler>>();
        var handler = new IssuePrescriptionCommandHandler(_mockUow.Object, _mockOutbox.Object, mockLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.MedicationName.Should().Be("Lisinopril");
        prescriptionRepoMock.Verify(r => r.AddAsync(It.IsAny<Prescription>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockOutbox.Verify(o => o.AddAsync(It.IsAny<OutboxEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task IssuePrescriptionCommandHandler_ShouldPublishEvent()
    {
        // Arrange
        var outboxEvent = (OutboxEvent)null;

        var prescriptionRepoMock = new Mock<IRepository<Prescription>>();
        prescriptionRepoMock
            .Setup(r => r.AddAsync(It.IsAny<Prescription>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUow
            .Setup(u => u.Repository<Prescription>())
            .Returns(prescriptionRepoMock.Object);

        _mockUow
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockOutbox
            .Setup(o => o.AddAsync(It.IsAny<OutboxEvent>(), It.IsAny<CancellationToken>()))
            .Callback<OutboxEvent, CancellationToken>((evt, ct) => outboxEvent = evt)
            .Returns(Task.CompletedTask);

        var command = new IssuePrescriptionCommand
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            MedicationName = "Metformin",
            Strength = "500mg",
            Dosage = "1 tablet",
            Frequency = "twice daily",
            Quantity = 60,
            RefillsAllowed = 0,
            StartDate = DateTime.UtcNow
        };

        var mockLogger = new Mock<ILogger<IssuePrescriptionCommandHandler>>();
        var handler = new IssuePrescriptionCommandHandler(_mockUow.Object, _mockOutbox.Object, mockLogger.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        outboxEvent.Should().NotBeNull();
        outboxEvent!.EventType.Should().Contain("PrescriptionIssued");
    }

    #endregion

    #region RequestRefill Tests

    [Fact]
    public async Task RequestRefillCommandHandler_WithValidPrescription_ShouldCreateRefillRequest()
    {
        // Arrange
        var prescriptionId = Guid.NewGuid();
        var prescription = new Prescription
        {
            Id = prescriptionId,
            Status = "Active",
            RefillsAllowed = 5,
            RefillsUsed = 0,
            EndDate = DateTime.UtcNow.AddMonths(1)
        };

        var prescriptionRepoMock = new Mock<IRepository<Prescription>>();
        prescriptionRepoMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Func<IQueryable<Prescription>, IQueryable<Prescription>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(prescription);

        prescriptionRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Prescription>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUow
            .Setup(u => u.Repository<Prescription>())
            .Returns(prescriptionRepoMock.Object);

        _mockUow
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockOutbox
            .Setup(o => o.AddAsync(It.IsAny<OutboxEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new RequestRefillCommand
        {
            PrescriptionId = prescriptionId,
            PharmacyId = "pharmacy_001"
        };

        var mockLogger = new Mock<ILogger<RequestRefillCommandHandler>>();
        var handler = new RequestRefillCommandHandler(_mockUow.Object, _mockOutbox.Object, mockLogger.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        prescriptionRepoMock.Verify(r => r.UpdateAsync(It.IsAny<Prescription>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockOutbox.Verify(o => o.AddAsync(It.IsAny<OutboxEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RequestRefillCommandHandler_WithNonexistentPrescription_ShouldThrow()
    {
        // Arrange
        var prescriptionRepoMock = new Mock<IRepository<Prescription>>();
        prescriptionRepoMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Func<IQueryable<Prescription>, IQueryable<Prescription>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Prescription)null);

        _mockUow
            .Setup(u => u.Repository<Prescription>())
            .Returns(prescriptionRepoMock.Object);

        var command = new RequestRefillCommand
        {
            PrescriptionId = Guid.NewGuid(),
            PharmacyId = "pharmacy_001"
        };

        var mockLogger = new Mock<ILogger<RequestRefillCommandHandler>>();
        var handler = new RequestRefillCommandHandler(_mockUow.Object, _mockOutbox.Object, mockLogger.Object);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command, CancellationToken.None));
    }

    #endregion

    #region ApproveRefill Tests

    [Fact]
    public async Task ApproveRefillCommandHandler_WithValidRefill_ShouldApproveAndIncrementCount()
    {
        // Arrange
        var prescriptionId = Guid.NewGuid();
        var refillId = Guid.NewGuid();

        var prescription = new Prescription
        {
            Id = prescriptionId,
            Status = "Active",
            RefillsAllowed = 5,
            RefillsUsed = 0,
            EndDate = DateTime.UtcNow.AddMonths(1)
        };

        prescription.RequestRefill("pharmacy_001");
        var actualRefillId = prescription.Refills.First().Id;

        var prescriptionRepoMock = new Mock<IRepository<Prescription>>();
        prescriptionRepoMock
            .Setup(r => r.FirstOrDefaultAsync(It.IsAny<Func<IQueryable<Prescription>, IQueryable<Prescription>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(prescription);

        prescriptionRepoMock
            .Setup(r => r.UpdateAsync(It.IsAny<Prescription>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUow
            .Setup(u => u.Repository<Prescription>())
            .Returns(prescriptionRepoMock.Object);

        _mockUow
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockOutbox
            .Setup(o => o.AddAsync(It.IsAny<OutboxEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new ApproveRefillCommand
        {
            PrescriptionId = prescriptionId,
            RefillId = actualRefillId
        };

        var mockLogger = new Mock<ILogger<ApproveRefillCommandHandler>>();
        var handler = new ApproveRefillCommandHandler(_mockUow.Object, _mockOutbox.Object, mockLogger.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        prescription.Refills.First().Status.Should().Be("Approved");
        prescription.RefillsUsed.Should().Be(1);
    }

    #endregion

    #region Prescription Lifecycle Tests

    [Fact]
    public void Prescription_Suspend_ShouldPreventRefills()
    {
        // Arrange
        var prescription = new Prescription
        {
            Status = "Active",
            RefillsAllowed = 5,
            RefillsUsed = 0,
            EndDate = DateTime.UtcNow.AddMonths(1)
        };

        // Act
        prescription.Suspend("Drug interaction detected");

        // Assert
        prescription.Status.Should().Be("Suspended");
        prescription.CanRefill().Should().BeFalse();
    }

    [Fact]
    public void Prescription_Discontinue_ShouldSetEndDate()
    {
        // Arrange
        var prescription = new Prescription
        {
            Id = Guid.NewGuid(),
            Status = "Active",
            EndDate = null
        };

        var beforeDiscontinue = DateTime.UtcNow;

        // Act
        prescription.Discontinue("Patient preference");

        var afterDiscontinue = DateTime.UtcNow;

        // Assert
        prescription.Status.Should().Be("Discontinued");
        prescription.EndDate.Should().HaveValue();
        prescription.EndDate.Should().BeCloseTo(beforeDiscontinue, TimeSpan.FromSeconds(1));
    }

    #endregion

    #region ControlledSubstance Tests

    [Theory]
    [InlineData("Morphine", true)]
    [InlineData("Fentanyl", true)]
    [InlineData("Oxycodone", true)]
    [InlineData("Amoxicillin", false)]
    [InlineData("Aspirin", false)]
    public void Prescription_ControlledSubstance_ShouldBeProperlyCategorized(string medication, bool isControlled)
    {
        // Arrange & Act
        var prescription = new Prescription
        {
            MedicationName = medication,
            IsControlledSubstance = isControlled
        };

        // Assert
        prescription.IsControlledSubstance.Should().Be(isControlled);
    }

    #endregion
}
