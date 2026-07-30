using EHRPlatform.Common.Data.Abstractions;
using EHRPlatform.Services.Audit.Domain.Entities;
using EHRPlatform.Services.Audit.Features.Audit.Commands;
using EHRPlatform.Services.Audit.Features.Audit.Dtos.Responses;
using EHRPlatform.Tests.Common.Base;
using FluentAssertions;
using Moq;
using Xunit;

namespace EHRPlatform.Tests.Unit.Services;

/// <summary>
/// Unit tests for Audit service command handlers.
/// Tests audit entry recording, compliance reporting, export functionality.
/// </summary>
public class AuditServiceTests : UnitTestBase
{
    private readonly Mock<IUnitOfWork> _mockUow;
    private readonly Mock<ILogger<RecordAuditEntryCommandHandler>> _mockLogger;

    public AuditServiceTests()
    {
        _mockUow = new Mock<IUnitOfWork>();
        _mockLogger = new Mock<ILogger<RecordAuditEntryCommandHandler>>();
    }

    #region RecordAuditEntry Tests

    [Fact]
    public async Task RecordAuditEntryCommandHandler_WithValidCommand_ShouldCreateAuditEntry()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var userEmail = "user@example.com";

        var auditRepoMock = new Mock<IRepository<AuditEntry>>();
        auditRepoMock
            .Setup(r => r.AddAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUow
            .Setup(u => u.Repository<AuditEntry>())
            .Returns(auditRepoMock.Object);

        _mockUow
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new RecordAuditEntryCommand
        {
            UserId = userId,
            UserEmail = userEmail,
            Action = "Read",
            ResourceType = "Patient",
            ResourceId = resourceId,
            IpAddress = "192.168.1.1",
            UserAgent = "Test-Agent/1.0",
            Success = true
        };

        var handler = new RecordAuditEntryCommandHandler(_mockUow.Object, _mockLogger.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        auditRepoMock.Verify(r => r.AddAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordAuditEntryCommandHandler_ShouldComputeIntegrityHash()
    {
        // Arrange
        var auditEntry = (AuditEntry)null;

        var auditRepoMock = new Mock<IRepository<AuditEntry>>();
        auditRepoMock
            .Setup(r => r.AddAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEntry, CancellationToken>((entry, ct) => auditEntry = entry)
            .Returns(Task.CompletedTask);

        _mockUow
            .Setup(u => u.Repository<AuditEntry>())
            .Returns(auditRepoMock.Object);

        _mockUow
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new RecordAuditEntryCommand
        {
            UserId = Guid.NewGuid(),
            UserEmail = "user@example.com",
            Action = "Update",
            ResourceType = "Patient",
            ResourceId = Guid.NewGuid()
        };

        var handler = new RecordAuditEntryCommandHandler(_mockUow.Object, _mockLogger.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        auditEntry.Should().NotBeNull();
        auditEntry!.IntegrityHash.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task RecordAuditEntryCommandHandler_WithPiiIndicators_ShouldRecordAccess()
    {
        // Arrange
        var auditEntry = (AuditEntry)null;

        var auditRepoMock = new Mock<IRepository<AuditEntry>>();
        auditRepoMock
            .Setup(r => r.AddAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEntry, CancellationToken>((entry, ct) => auditEntry = entry)
            .Returns(Task.CompletedTask);

        _mockUow
            .Setup(u => u.Repository<AuditEntry>())
            .Returns(auditRepoMock.Object);

        _mockUow
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new RecordAuditEntryCommand
        {
            UserId = Guid.NewGuid(),
            UserEmail = "user@example.com",
            Action = "Read",
            ResourceType = "Patient",
            ResourceId = Guid.NewGuid(),
            PiiIndicators = "SSN,DOB,MRN"
        };

        var handler = new RecordAuditEntryCommandHandler(_mockUow.Object, _mockLogger.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        auditEntry!.PiiIndicators.Should().Contain("SSN");
    }

    [Fact]
    public async Task RecordAuditEntryCommandHandler_WithFailedAction_ShouldRecordFailureReason()
    {
        // Arrange
        var auditEntry = (AuditEntry)null;

        var auditRepoMock = new Mock<IRepository<AuditEntry>>();
        auditRepoMock
            .Setup(r => r.AddAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEntry, CancellationToken>((entry, ct) => auditEntry = entry)
            .Returns(Task.CompletedTask);

        _mockUow
            .Setup(u => u.Repository<AuditEntry>())
            .Returns(auditRepoMock.Object);

        _mockUow
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new RecordAuditEntryCommand
        {
            UserId = Guid.NewGuid(),
            UserEmail = "user@example.com",
            Action = "Delete",
            ResourceType = "User",
            ResourceId = Guid.NewGuid(),
            Success = false,
            FailureReason = "Unauthorized access"
        };

        var handler = new RecordAuditEntryCommandHandler(_mockUow.Object, _mockLogger.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        auditEntry!.Status.Should().Be("Failure");
        auditEntry.FailureReason.Should().Be("Unauthorized access");
    }

    [Fact]
    public async Task RecordAuditEntryCommandHandler_WithRestrictedAccessLevel_ShouldRecordConfidentiality()
    {
        // Arrange
        var auditEntry = (AuditEntry)null;

        var auditRepoMock = new Mock<IRepository<AuditEntry>>();
        auditRepoMock
            .Setup(r => r.AddAsync(It.IsAny<AuditEntry>(), It.IsAny<CancellationToken>()))
            .Callback<AuditEntry, CancellationToken>((entry, ct) => auditEntry = entry)
            .Returns(Task.CompletedTask);

        _mockUow
            .Setup(u => u.Repository<AuditEntry>())
            .Returns(auditRepoMock.Object);

        _mockUow
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new RecordAuditEntryCommand
        {
            UserId = Guid.NewGuid(),
            UserEmail = "user@example.com",
            Action = "Read",
            ResourceType = "SensitiveData",
            ResourceId = Guid.NewGuid(),
            AccessLevel = 4 // Restricted
        };

        var handler = new RecordAuditEntryCommandHandler(_mockUow.Object, _mockLogger.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        auditEntry!.AccessLevel.Should().Be(4);
    }

    #endregion

    #region RecordDataChange Tests

    [Fact]
    public async Task RecordDataChangeCommandHandler_WithValidCommand_ShouldCreateDataChangeAudit()
    {
        // Arrange
        var dataChangeRepoMock = new Mock<IRepository<DataChangeAudit>>();
        dataChangeRepoMock
            .Setup(r => r.AddAsync(It.IsAny<DataChangeAudit>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _mockUow
            .Setup(u => u.Repository<DataChangeAudit>())
            .Returns(dataChangeRepoMock.Object);

        _mockUow
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new RecordDataChangeCommand
        {
            UserId = Guid.NewGuid(),
            ResourceType = "Patient",
            ResourceId = Guid.NewGuid(),
            FieldName = "Email",
            OldValue = "old@example.com",
            NewValue = "new@example.com",
            Reason = "User requested change"
        };

        var mockLogger = new Mock<ILogger<RecordDataChangeCommandHandler>>();
        var handler = new RecordDataChangeCommandHandler(_mockUow.Object, mockLogger.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        dataChangeRepoMock.Verify(r => r.AddAsync(It.IsAny<DataChangeAudit>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordDataChangeCommandHandler_ShouldCaptureBeforeAndAfterValues()
    {
        // Arrange
        var dataChange = (DataChangeAudit)null;

        var dataChangeRepoMock = new Mock<IRepository<DataChangeAudit>>();
        dataChangeRepoMock
            .Setup(r => r.AddAsync(It.IsAny<DataChangeAudit>(), It.IsAny<CancellationToken>()))
            .Callback<DataChangeAudit, CancellationToken>((change, ct) => dataChange = change)
            .Returns(Task.CompletedTask);

        _mockUow
            .Setup(u => u.Repository<DataChangeAudit>())
            .Returns(dataChangeRepoMock.Object);

        _mockUow
            .Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new RecordDataChangeCommand
        {
            UserId = Guid.NewGuid(),
            ResourceType = "User",
            ResourceId = Guid.NewGuid(),
            FieldName = "Status",
            OldValue = "Active",
            NewValue = "Inactive"
        };

        var mockLogger = new Mock<ILogger<RecordDataChangeCommandHandler>>();
        var handler = new RecordDataChangeCommandHandler(_mockUow.Object, mockLogger.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        dataChange!.OldValue.Should().Be("Active");
        dataChange.NewValue.Should().Be("Inactive");
        dataChange.FieldName.Should().Be("Status");
    }

    #endregion

    #region GenerateComplianceReport Tests

    [Fact]
    public void ComplianceReport_Create_ShouldInitializeWithCorrectPeriod()
    {
        // Arrange
        var startDate = new DateTime(2026, 01, 01);
        var endDate = new DateTime(2026, 01, 31);

        // Act
        var report = new ComplianceReport
        {
            PeriodStart = startDate,
            PeriodEnd = endDate,
            TotalActions = 1000,
            FailedActions = 5,
            Status = "Generated"
        };

        // Assert
        report.PeriodStart.Should().Be(startDate);
        report.PeriodEnd.Should().Be(endDate);
        report.TotalActions.Should().Be(1000);
        report.FailedActions.Should().Be(5);
    }

    [Fact]
    public void ComplianceReport_DigitalSignature_ShouldTrackReviewAndSign()
    {
        // Arrange
        var report = new ComplianceReport
        {
            PeriodStart = DateTime.UtcNow.AddMonths(-1),
            PeriodEnd = DateTime.UtcNow,
            Status = "Generated"
        };

        var signatureTime = DateTime.UtcNow;

        // Act
        report.Status = "Signed";
        report.SignedBy = "compliance_officer@example.com";
        report.SignedAt = signatureTime;
        report.DigitalSignature = "signature_hash_value";

        // Assert
        report.Status.Should().Be("Signed");
        report.SignedBy.Should().Be("compliance_officer@example.com");
        report.SignedAt.Should().Be(signatureTime);
        report.DigitalSignature.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ComplianceReport_ShouldTrackPiiAccessed()
    {
        // Arrange
        var piiAccessList = new List<string> { "SSN", "DOB", "MRN", "HealthInfo" };

        // Act
        var report = new ComplianceReport
        {
            PeriodStart = DateTime.UtcNow.AddMonths(-1),
            PeriodEnd = DateTime.UtcNow,
            PiiAccessed = piiAccessList
        };

        // Assert
        report.PiiAccessed.Should().HaveCount(4);
        report.PiiAccessed.Should().Contain("SSN");
        report.PiiAccessed.Should().Contain("HealthInfo");
    }

    #endregion

    #region AuditLogExport Tests

    [Fact]
    public void AuditLogExport_Create_ShouldRecordExportDetails()
    {
        // Arrange
        var exportedBy = Guid.NewGuid();
        var exportTime = DateTime.UtcNow;

        // Act
        var export = new AuditLogExport
        {
            ExportedAt = exportTime,
            ExportedBy = exportedBy,
            PeriodStart = DateTime.UtcNow.AddMonths(-1),
            PeriodEnd = DateTime.UtcNow,
            RecordCount = 5000,
            Format = "PDF",
            IsEncrypted = true,
            Status = "Completed",
            FileHash = "sha256_hash"
        };

        // Assert
        export.ExportedAt.Should().Be(exportTime);
        export.ExportedBy.Should().Be(exportedBy);
        export.RecordCount.Should().Be(5000);
        export.Format.Should().Be("PDF");
        export.IsEncrypted.Should().BeTrue();
        export.FileHash.Should().NotBeNullOrEmpty();
    }

    [Theory]
    [InlineData("PDF")]
    [InlineData("CSV")]
    [InlineData("JSON")]
    public void AuditLogExport_ShouldSupportMultipleFormats(string format)
    {
        // Arrange & Act
        var export = new AuditLogExport
        {
            Format = format,
            ExportedAt = DateTime.UtcNow,
            ExportedBy = Guid.NewGuid()
        };

        // Assert
        export.Format.Should().Be(format);
    }

    #endregion
}
