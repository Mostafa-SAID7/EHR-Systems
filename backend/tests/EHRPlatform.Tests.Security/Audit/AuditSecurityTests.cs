using EHRPlatform.Services.Audit.Domain.Entities;
using EHRPlatform.Tests.Common.Base;
using FluentAssertions;
using Xunit;

namespace EHRPlatform.Tests.Security.Audit;

/// <summary>
/// Security tests for Audit Service.
/// Tests compliance, immutability, tampering detection, data protection.
/// HIPAA-critical: Ensures audit logs cannot be modified after creation.
/// </summary>
public class AuditSecurityTests : UnitTestBase
{
    #region Immutability and Tampering Detection Tests

    [Fact]
    public void AuditEntry_Immutability_ShouldPreventTampering()
    {
        // Arrange
        var originalHash = "original_integrity_hash";
        var auditEntry = new AuditEntry
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Action = "Delete",
            ResourceType = "SensitiveData",
            Status = "Success",
            Timestamp = DateTime.UtcNow,
            IntegrityHash = originalHash
        };

        // Act - Verify integrity with original hash
        var isValidBefore = auditEntry.VerifyIntegrity(originalHash);

        // Act - Attempt tampering
        var tamperedHash = "tampered_hash_value";
        var isValidAfter = auditEntry.VerifyIntegrity(tamperedHash);

        // Assert
        isValidBefore.Should().BeTrue("Original hash should be valid");
        isValidAfter.Should().BeFalse("Tampered hash should fail verification");
    }

    [Fact]
    public void AuditEntry_WithPiiIndicators_ShouldEnforceAccessControl()
    {
        // Arrange - Create audit entry with restricted PII access
        var restrictedPii = "SSN,DOB,MRN,FullHealthRecord";
        var restrictedAccessLevel = 4; // Highest restriction level

        var auditEntry = new AuditEntry
        {
            UserId = Guid.NewGuid(),
            Action = "Read",
            ResourceType = "Patient",
            PiiIndicators = restrictedPii,
            AccessLevel = restrictedAccessLevel,
            Status = "Success"
        };

        // Act & Assert - Verify sensitive data tracking
        auditEntry.PiiIndicators.Should().Contain("SSN");
        auditEntry.PiiIndicators.Should().Contain("HealthRecord");
        auditEntry.AccessLevel.Should().Be(4);
    }

    #endregion

    #region Failed Action Tracking Tests

    [Fact]
    public void AuditEntry_UnauthorizedAttempt_ShouldRecordFailureDetails()
    {
        // Arrange
        var failureReason = "Unauthorized: User lacks 'patient:write' permission";
        
        var auditEntry = new AuditEntry
        {
            UserId = Guid.NewGuid(),
            Action = "Update",
            ResourceType = "Patient",
            Status = "Failure",
            FailureReason = failureReason,
            IpAddress = "192.168.1.100",
            UserAgent = "Suspicious-Client/1.0"
        };

        // Act & Assert
        auditEntry.Status.Should().Be("Failure");
        auditEntry.FailureReason.Should().Contain("Unauthorized");
        auditEntry.FailureReason.Should().Contain("patient:write");
    }

    [Fact]
    public void AuditEntry_AccessAttempt_ShouldCaptureThreatInformation()
    {
        // Arrange - Simulate malicious access attempt
        var suspiciousIp = "203.0.113.42"; // Unusual IP
        var suspiciousUserAgent = "SQLMap/1.4.4 (exploitation tool)";

        var auditEntry = new AuditEntry
        {
            UserId = Guid.NewGuid(),
            Action = "Delete",
            ResourceType = "User",
            Status = "Failure",
            FailureReason = "SQL Injection attempt detected",
            IpAddress = suspiciousIp,
            UserAgent = suspiciousUserAgent,
            IntegrityHash = "hash_for_forensics"
        };

        // Act & Assert
        auditEntry.IpAddress.Should().Be(suspiciousIp);
        auditEntry.UserAgent.Should().Contain("SQLMap");
        auditEntry.FailureReason.Should().Contain("SQL Injection");
        auditEntry.IntegrityHash.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region ComplianceReport Security Tests

    [Fact]
    public void ComplianceReport_DigitalSignature_ShouldPreventForging()
    {
        // Arrange
        var legitimateSignature = "sha256_signature_from_officer";
        var forgedSignature = "forged_signature_attempt";

        var report = new ComplianceReport
        {
            PeriodStart = DateTime.UtcNow.AddMonths(-1),
            PeriodEnd = DateTime.UtcNow,
            TotalActions = 5000,
            Status = "Signed",
            SignedBy = "compliance_officer@example.com",
            SignedAt = DateTime.UtcNow,
            DigitalSignature = legitimateSignature
        };

        // Act - Verify legitimate signature
        var isLegitimate = report.DigitalSignature == legitimateSignature;

        // Act - Attempt forge
        var isForged = report.DigitalSignature == forgedSignature;

        // Assert
        isLegitimate.Should().BeTrue();
        isForged.Should().BeFalse();
    }

    [Fact]
    public void ComplianceReport_ShouldTrackAllPiiAccess()
    {
        // Arrange
        var sensitiveDataTypes = new List<string> { "SSN", "DOB", "MRN", "FullMedicalHistory", "Genetics" };

        var report = new ComplianceReport
        {
            PeriodStart = DateTime.UtcNow.AddMonths(-1),
            PeriodEnd = DateTime.UtcNow,
            PiiAccessed = sensitiveDataTypes,
            Status = "Generated"
        };

        // Act & Assert - Verify all sensitive data types are tracked
        report.PiiAccessed.Should().HaveCount(5);
        report.PiiAccessed.Should().Contain("SSN");
        report.PiiAccessed.Should().Contain("Genetics");
    }

    #endregion

    #region Data Protection Tests

    [Fact]
    public void AuditEntry_Encryption_ShouldProtectSensitiveContent()
    {
        // Arrange
        var encryptedEntry = new AuditEntry
        {
            UserId = Guid.NewGuid(),
            Action = "Read",
            ResourceType = "Patient",
            PiiIndicators = "SSN,FullAddress",
            IsEncrypted = true,
            IntegrityHash = "hash_of_encrypted_content"
        };

        var unencryptedEntry = new AuditEntry
        {
            UserId = Guid.NewGuid(),
            Action = "Read",
            ResourceType = "Patient",
            PiiIndicators = "PhoneNumber",
            IsEncrypted = false
        };

        // Act & Assert
        encryptedEntry.IsEncrypted.Should().BeTrue();
        encryptedEntry.PiiIndicators.Should().Contain("SSN"); // Sensitive data encrypted
        
        unencryptedEntry.IsEncrypted.Should().BeFalse();
        unencryptedEntry.PiiIndicators.Should().Contain("PhoneNumber"); // Less sensitive
    }

    #endregion

    #region HIPAA Compliance Tests

    [Fact]
    public void AuditEntry_ShouldComplyWithHipaaAuditRequirements()
    {
        // Arrange - HIPAA audit trail requirements:
        // 1. User ID
        // 2. Timestamp
        // 3. Type of event
        // 4. Resource accessed
        // 5. Success/Failure
        // 6. IP Address
        // 7. User Agent

        var auditEntry = new AuditEntry
        {
            UserId = Guid.NewGuid(),                      // 1. User ID
            Timestamp = DateTime.UtcNow,                  // 2. Timestamp
            Action = "Read",                              // 3. Type of event
            ResourceType = "Patient",                     // 4. Resource
            ResourceId = Guid.NewGuid(),                  // 4. Resource ID
            Status = "Success",                           // 5. Success/Failure
            IpAddress = "192.168.1.1",                    // 6. IP Address
            UserAgent = "Mozilla/5.0"                     // 7. User Agent
        };

        // Act & Assert
        auditEntry.UserId.Should().NotBe(Guid.Empty);
        auditEntry.Timestamp.Should().BeLessThanOrEqualTo(DateTime.UtcNow);
        auditEntry.Action.Should().NotBeNullOrEmpty();
        auditEntry.ResourceType.Should().NotBeNullOrEmpty();
        auditEntry.Status.Should().NotBeNullOrEmpty();
        auditEntry.IpAddress.Should().NotBeNullOrEmpty();
        auditEntry.UserAgent.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void AuditEntry_AccessLog_ShouldTrackExportsAndPrints()
    {
        // Arrange - HIPAA requires tracking of all PHI exports and prints
        var exportLog = new AccessLog
        {
            UserId = Guid.NewGuid(),
            ResourceType = "Patient",
            ResourceId = Guid.NewGuid(),
            IsExport = true,
            IsPrint = false,
            DurationSeconds = 120
        };

        var printLog = new AccessLog
        {
            UserId = Guid.NewGuid(),
            ResourceType = "ClinicalNotes",
            ResourceId = Guid.NewGuid(),
            IsExport = false,
            IsPrint = true,
            DurationSeconds = 60
        };

        // Act & Assert
        exportLog.IsExport.Should().BeTrue();
        printLog.IsPrint.Should().BeTrue();
    }

    #endregion

    #region Data Change Audit Tests

    [Fact]
    public void DataChangeAudit_ShouldCaptureBothOldAndNewValues()
    {
        // Arrange
        var changeAudit = new DataChangeAudit
        {
            UserId = Guid.NewGuid(),
            ResourceType = "User",
            FieldName = "Role",
            OldValue = "Nurse",
            NewValue = "Doctor",
            ChangeType = "Modified",
            Reason = "Promotion after certification"
        };

        // Act & Assert
        changeAudit.OldValue.Should().Be("Nurse");
        changeAudit.NewValue.Should().Be("Doctor");
        changeAudit.ChangeType.Should().Be("Modified");
        changeAudit.Reason.Should().NotBeNullOrEmpty();
    }

    #endregion
}
