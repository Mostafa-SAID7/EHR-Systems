#nullable enable

using System;
using Xunit;
using FluentAssertions;
using EHRPlatform.Tests.Common.Builders;
using EHRPlatform.Tests.Common.Helpers;

namespace EHRPlatform.Tests.Security.DataProtection;

/// <summary>
/// Tests for PHI (Protected Health Information) protection.
/// Verifies encryption, access control, and masking of sensitive data.
/// HIPAA-compliance focused.
/// </summary>
public class PhiProtectionTests
{
    [Fact]
    public void PatientPHI_FieldsIdentified_Correctly()
    {
        // Arrange
        var phiFields = new[] { "phone", "ssn", "date_of_birth", "email", "address" };

        // Act & Assert
        foreach (var field in phiFields)
        {
            HipaaComplianceHelper.IsPHIField(field).Should().BeTrue();
        }
    }

    [Fact]
    public void NonPHI_FieldsNotIdentified_AsPhI()
    {
        // Arrange
        var nonPhiFields = new[] { "is_active", "created_at", "id" };

        // Act & Assert
        foreach (var field in nonPhiFields)
        {
            HipaaComplianceHelper.IsPHIField(field).Should().BeFalse();
        }
    }

    [Fact]
    public void EncryptPHI_WithAes256_ProducesEncryptedData()
    {
        // Arrange
        var patientData = "John Doe, DOB: 1980-01-01";
        var (key, iv) = HipaaComplianceHelper.GenerateEncryptionKeyPair();

        // Act
        var encrypted = HipaaComplianceHelper.EncryptPHI(patientData, key, iv);

        // Assert
        encrypted.Should().NotBeEmpty();
        encrypted.Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public void DecryptPHI_WithCorrectKey_RecoveryData()
    {
        // Arrange
        var originalData = "John Doe, DOB: 1980-01-01";
        var (key, iv) = HipaaComplianceHelper.GenerateEncryptionKeyPair();

        var encrypted = HipaaComplianceHelper.EncryptPHI(originalData, key, iv);

        // Act
        var decrypted = HipaaComplianceHelper.DecryptPHI(encrypted, key, iv);

        // Assert
        decrypted.Should().Be(originalData);
    }

    [Fact]
    public void DecryptPHI_WithWrongKey_Fails()
    {
        // Arrange
        var patientData = "John Doe, DOB: 1980-01-01";
        var (key1, iv1) = HipaaComplianceHelper.GenerateEncryptionKeyPair();
        var (key2, iv2) = HipaaComplianceHelper.GenerateEncryptionKeyPair();

        var encrypted = HipaaComplianceHelper.EncryptPHI(patientData, key1, iv1);

        // Act & Assert
        var action = () => HipaaComplianceHelper.DecryptPHI(encrypted, key2, iv2);
        action.Should().Throw<Exception>();
    }

    [Fact]
    public void MaskPhi_RemovesMiddle_KeepsEnds()
    {
        // Arrange
        var phi = "1234567890";

        // Act
        var masked = HipaaComplianceHelper.MaskPHI(phi);

        // Assert
        masked.Should().Contain("12");
        masked.Should().Contain("90");
        masked.Should().Contain("****");
        masked.Should().StartWith("12");
        masked.Should().EndWith("90");
    }

    [Fact]
    public void MaskPHI_WithShortString_MasksCompletely()
    {
        // Arrange
        var phi = "123";

        // Act
        var masked = HipaaComplianceHelper.MaskPHI(phi);

        // Assert
        masked.Should().Be("****");
    }

    [Fact]
    public void PatientData_Validation_ConfirmsHipaaRequirements()
    {
        // Arrange
        var (key, iv) = HipaaComplianceHelper.GenerateEncryptionKeyPair();
        var patientData = HipaaComplianceHelper.GenerateSyntheticPatientData();

        // Act & Assert
        patientData.Should().ContainKey("first_name");
        patientData.Should().ContainKey("phone");
        patientData.Should().ContainKey("email");
        patientData.Should().ContainKey("mrn");

        // Verify no hardcoded PHI
        var content = System.Text.Json.JsonSerializer.Serialize(patientData);
        HipaaComplianceHelper.ValidateNoHardcodedPHI(content).Should().BeTrue();
    }

    [Theory]
    [InlineData("123-45-6789")]
    [InlineData("123456789")]
    public void HardcodedSSN_IsDetected(string ssnPattern)
    {
        // Arrange & Act
        var isHardcoded = System.Text.RegularExpressions.Regex.IsMatch(
            ssnPattern,
            @"\b\d{3}-\d{2}-\d{4}\b");

        // Assert
        isHardcoded.Should().BeTrue();
    }

    [Fact]
    public void AuditLogCreated_ForPhiAccess()
    {
        // Arrange
        var auditEntry = new System.Collections.Generic.Dictionary<string, object>
        {
            { "id", Guid.NewGuid() },
            { "timestamp", DateTime.UtcNow },
            { "user_id", Guid.NewGuid() },
            { "action", "ViewPatientPHI" },
            { "entity_type", "Patient" },
            { "entity_id", Guid.NewGuid() },
            { "changes", "Viewed phone number" }
        };

        // Act & Assert
        HipaaComplianceHelper.ValidateAuditTrail(auditEntry).Should().BeTrue();
    }

    [Fact]
    public void DataRetention_Policy_IsHonored()
    {
        // Arrange
        var createdDate = DateTime.UtcNow.AddYears(-5);

        // Act
        var isWithinRetention = HipaaComplianceHelper.ValidateDataRetention(createdDate, retentionYears: 6);

        // Assert
        isWithinRetention.Should().BeTrue();
    }

    [Fact]
    public void DataRetention_Expired_FailsValidation()
    {
        // Arrange
        var createdDate = DateTime.UtcNow.AddYears(-7);

        // Act
        var isWithinRetention = HipaaComplianceHelper.ValidateDataRetention(createdDate, retentionYears: 6);

        // Assert
        isWithinRetention.Should().BeFalse();
    }

    [Fact]
    public void ConsentRecord_Contains_RequiredFields()
    {
        // Arrange
        var consentRecord = new System.Collections.Generic.Dictionary<string, object>
        {
            { "patient_id", Guid.NewGuid() },
            { "consent_type", "DataSharing" },
            { "consent_date", DateTime.UtcNow },
            { "expiration_date", DateTime.UtcNow.AddYears(1) }
        };

        // Act & Assert
        HipaaComplianceHelper.ValidateConsentTracking(consentRecord).Should().BeTrue();
    }

    [Fact]
    public void ConsentRecord_Missing_RequiredField_Fails()
    {
        // Arrange
        var incompleteConsent = new System.Collections.Generic.Dictionary<string, object>
        {
            { "patient_id", Guid.NewGuid() },
            { "consent_type", "DataSharing" }
            // Missing consent_date and expiration_date
        };

        // Act & Assert
        HipaaComplianceHelper.ValidateConsentTracking(incompleteConsent).Should().BeFalse();
    }
}
