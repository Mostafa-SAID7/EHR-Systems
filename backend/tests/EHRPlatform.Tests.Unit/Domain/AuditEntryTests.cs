using EHRPlatform.Services.Audit.Domain.Entities;
using EHRPlatform.Services.Audit.Domain.Enums;
using EHRPlatform.Tests.Common.Base;
using FluentAssertions;
using System.Security.Cryptography;
using System.Text;
using Xunit;

namespace EHRPlatform.Tests.Unit.Domain;

/// <summary>
/// Unit tests for AuditEntry domain entity.
/// Tests immutability, integrity verification, HIPAA compliance.
/// </summary>
public class AuditEntryTests : UnitTestBase
{
    [Fact]
    public void AuditEntry_Create_ShouldInitializeWithCorrectValues()
    {
        // Arrange
        var auditId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var resourceId = Guid.NewGuid();
        var userEmail = "user@example.com";
        var action = "Read";
        var resourceType = "Patient";
        var timestamp = DateTime.UtcNow;

        // Act
        var auditEntry = new AuditEntry
        {
            Id = auditId,
            UserId = userId,
            UserEmail = userEmail,
            Action = action,
            ResourceType = resourceType,
            ResourceId = resourceId,
            Status = "Success",
            Timestamp = timestamp,
            IpAddress = "192.168.1.1",
            UserAgent = "Mozilla/5.0",
            PiiIndicators = "SSN,DOB",
            AccessLevel = 3,
            IntegrityHash = "hash_value"
        };

        // Assert
        auditEntry.Id.Should().Be(auditId);
        auditEntry.UserId.Should().Be(userId);
        auditEntry.UserEmail.Should().Be(userEmail);
        auditEntry.Action.Should().Be(action);
        auditEntry.ResourceType.Should().Be(resourceType);
        auditEntry.ResourceId.Should().Be(resourceId);
        auditEntry.Status.Should().Be("Success");
        auditEntry.Timestamp.Should().Be(timestamp);
        auditEntry.IpAddress.Should().Be("192.168.1.1");
        auditEntry.PiiIndicators.Should().Contain("SSN");
    }

    [Fact]
    public void AuditEntry_VerifyIntegrity_WithMatchingHash_ShouldReturnTrue()
    {
        // Arrange
        var correctHash = "sha256_hash_value";
        var auditEntry = new AuditEntry
        {
            UserId = Guid.NewGuid(),
            Action = "Update",
            ResourceType = "Patient",
            IntegrityHash = correctHash
        };

        // Act
        var isValid = auditEntry.VerifyIntegrity(correctHash);

        // Assert
        isValid.Should().BeTrue();
    }

    [Fact]
    public void AuditEntry_VerifyIntegrity_WithTamperedHash_ShouldReturnFalse()
    {
        // Arrange
        var originalHash = "original_sha256_hash";
        var tamperedHash = "tampered_hash_value";
        var auditEntry = new AuditEntry
        {
            UserId = Guid.NewGuid(),
            Action = "Update",
            ResourceType = "Patient",
            IntegrityHash = originalHash
        };

        // Act
        var isValid = auditEntry.VerifyIntegrity(tamperedHash);

        // Assert
        isValid.Should().BeFalse();
    }

    [Fact]
    public void AuditEntry_WithSuccessfulAction_ShouldHaveCorrectStatus()
    {
        // Arrange & Act
        var auditEntry = new AuditEntry
        {
            UserId = Guid.NewGuid(),
            Action = "Create",
            ResourceType = "Prescription",
            Status = "Success"
        };

        // Assert
        auditEntry.Status.Should().Be("Success");
    }

    [Fact]
    public void AuditEntry_WithFailedAction_ShouldHaveCorrectStatusAndReason()
    {
        // Arrange & Act
        var auditEntry = new AuditEntry
        {
            UserId = Guid.NewGuid(),
            Action = "Delete",
            ResourceType = "Patient",
            Status = "Failure",
            FailureReason = "Unauthorized access"
        };

        // Assert
        auditEntry.Status.Should().Be("Failure");
        auditEntry.FailureReason.Should().Be("Unauthorized access");
    }

    [Fact]
    public void AuditEntry_WithPiiIndicators_ShouldTrackAccessedData()
    {
        // Arrange
        var piiIndicators = "SSN,DOB,MRN,HealthInfo";

        // Act
        var auditEntry = new AuditEntry
        {
            UserId = Guid.NewGuid(),
            Action = "Read",
            ResourceType = "Patient",
            PiiIndicators = piiIndicators
        };

        // Assert
        auditEntry.PiiIndicators.Should().Contain("SSN");
        auditEntry.PiiIndicators.Should().Contain("DOB");
        auditEntry.PiiIndicators.Should().Contain("MRN");
    }

    [Fact]
    public void AuditEntry_WithAccessLevel_ShouldEnforceConfidentiality()
    {
        // Arrange
        var restrictedAccessLevel = 4; // Restricted

        // Act
        var auditEntry = new AuditEntry
        {
            UserId = Guid.NewGuid(),
            Action = "Read",
            ResourceType = "SensitiveData",
            AccessLevel = restrictedAccessLevel
        };

        // Assert
        auditEntry.AccessLevel.Should().Be(4);
        auditEntry.AccessLevel.Should().BeGreaterThanOrEqualTo(3); // Confidential or higher
    }

    [Fact]
    public void AuditEntry_WithChangeDetails_ShouldRecordDataModifications()
    {
        // Arrange
        var changeDetails = "{\"email\":{\"old\":\"old@test.com\",\"new\":\"new@test.com\"}}";

        // Act
        var auditEntry = new AuditEntry
        {
            UserId = Guid.NewGuid(),
            Action = "Update",
            ResourceType = "User",
            ChangeDetails = changeDetails
        };

        // Assert
        auditEntry.ChangeDetails.Should().Contain("old@test.com");
        auditEntry.ChangeDetails.Should().Contain("new@test.com");
    }

    [Fact]
    public void AuditEntry_WithSessionDuration_ShouldTrackAccessTime()
    {
        // Arrange
        var sessionDuration = 1800; // 30 minutes in seconds

        // Act
        var auditEntry = new AuditEntry
        {
            UserId = Guid.NewGuid(),
            Action = "Read",
            ResourceType = "Patient",
            SessionDurationSeconds = sessionDuration
        };

        // Assert
        auditEntry.SessionDurationSeconds.Should().Be(1800);
        auditEntry.SessionDurationSeconds.Should().BeLessThanOrEqualTo(3600); // Max 1 hour
    }

    [Fact]
    public void AuditEntry_IsEncrypted_ShouldIndicateDataProtection()
    {
        // Arrange & Act
        var auditEntry = new AuditEntry
        {
            UserId = Guid.NewGuid(),
            Action = "Read",
            ResourceType = "Patient",
            IsEncrypted = true
        };

        // Assert
        auditEntry.IsEncrypted.Should().BeTrue();
    }
}
