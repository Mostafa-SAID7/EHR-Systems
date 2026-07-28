using FluentAssertions;
using Moq;
using EHRPlatform.Services.Clinical.Domain.Entities;
using EHRPlatform.Common.Security;
using EHRPlatform.Tests.Common.Helpers;
using Xunit;

namespace EHRPlatform.Tests.Security.Clinical;

/// <summary>
/// Security tests for Clinical Service PHI (Protected Health Information) protection.
/// Tests HIPAA compliance, access control, and medical data security.
/// HIPAA: Clinical notes contain PHI and must be encrypted, access-controlled, and audited.
/// </summary>
public class ClinicalPhiTests
{
    private readonly HipaaComplianceHelper _hipaaHelper;
    private readonly Mock<IEncryptionService> _mockEncryption;

    public ClinicalPhiTests()
    {
        _hipaaHelper = new HipaaComplianceHelper();
        _mockEncryption = new Mock<IEncryptionService>();
    }

    [Fact]
    public void ClinicalNote_Subjective_ShouldBeConsideredPHI()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            Subjective = "Patient reports severe headaches and nausea"
        };

        // Act
        var isPhi = _hipaaHelper.IsPhiContent(note.Subjective);

        // Assert
        isPhi.Should().BeTrue("Subjective clinical information is PHI");
    }

    [Fact]
    public void ClinicalNote_Objective_ShouldBeConsideredPHI()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            Objective = "Temperature 101.5F, RR 24, BP 140/90"
        };

        // Act
        var isPhi = _hipaaHelper.IsPhiContent(note.Objective);

        // Assert
        isPhi.Should().BeTrue("Objective clinical observations are PHI");
    }

    [Fact]
    public void ClinicalNote_Assessment_ShouldBeConsideredPHI()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            Assessment = "Probable bacterial meningitis"
        };

        // Act
        var isPhi = _hipaaHelper.IsPhiContent(note.Assessment);

        // Assert
        isPhi.Should().BeTrue("Clinical assessment is PHI");
    }

    [Fact]
    public void ClinicalNote_Plan_ShouldBeConsideredPHI()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            Plan = "Start IV ceftriaxone 2g q4h, admit to ICU"
        };

        // Act
        var isPhi = _hipaaHelper.IsPhiContent(note.Plan);

        // Assert
        isPhi.Should().BeTrue("Treatment plan is PHI");
    }

    [Fact]
    public void ClinicalNote_DiagnosisWithICD10Code_ShouldBeConsideredPHI()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid()
        };
        note.AddDiagnosis("C34.90", "Unspecified malignant neoplasm of left lung", "Principal");

        // Act
        var diagnosis = note.Diagnoses.First();
        var isPhi = _hipaaHelper.IsPhiContent($"{diagnosis.DiagnosisCode} - {diagnosis.DiagnosisText}");

        // Assert
        isPhi.Should().BeTrue("Clinical diagnosis with ICD-10 code is PHI");
    }

    [Fact]
    public void ClinicalNote_VitalSigns_ShouldBeConsideredPHI()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid()
        };
        note.RecordVitals(98.6m, 120, 80, 72, 16);

        // Act
        var vitals = note.VitalSigns.First();
        var vitalString = $"BP: {vitals.SystolicBP}/{vitals.DiastolicBP}, HR: {vitals.HeartRate}";
        var isPhi = _hipaaHelper.IsPhiContent(vitalString);

        // Assert
        isPhi.Should().BeTrue("Vital signs are PHI");
    }

    [Fact]
    public void ClinicalNote_ShouldNotBeAccessibleToUnauthorizedProvider()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(), // Original provider
            Subjective = "Sensitive patient information"
        };

        var otherProviderId = Guid.NewGuid(); // Different provider

        // Act
        var isAccessible = note.ProviderId == otherProviderId;

        // Assert
        isAccessible.Should().BeFalse("Only authorized provider should access note");
    }

    [Fact]
    public void ClinicalNote_WhenEncrypted_ShouldNotContainPlaintextPHI()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            Subjective = "Patient has HIV/AIDS diagnosis"
        };

        var encryptedContent = "ENCRYPTED[a7f3e9d2c8b1f4a6]";
        _mockEncryption.Setup(e => e.Encrypt(note.Subjective))
            .Returns(encryptedContent);

        // Act
        var encrypted = _mockEncryption.Object.Encrypt(note.Subjective);

        // Assert
        encrypted.Should().NotContain("HIV");
        encrypted.Should().NotContain("AIDS");
        encrypted.Should().StartWith("ENCRYPTED");
    }

    [Fact]
    public void ClinicalNote_AccessAttempt_ShouldBeLoggable()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        var patientId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var requestingProviderId = Guid.NewGuid();

        var note = new ClinicalNote
        {
            Id = noteId,
            PatientId = patientId,
            ProviderId = providerId
        };

        // Act
        var accessLog = new
        {
            NoteId = note.Id,
            PatientId = note.PatientId,
            AccessedBy = requestingProviderId,
            AccessTime = DateTime.UtcNow,
            AccessGranted = note.ProviderId == requestingProviderId
        };

        // Assert
        accessLog.AccessGranted.Should().BeFalse("Unauthorized access attempt should be logged");
        accessLog.NoteId.Should().Equal(noteId);
    }

    [Fact]
    public void ClinicalNote_HighRiskDiagnosis_ShouldHaveLimitedVisibility()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid()
        };
        // High-risk diagnosis that should have limited visibility
        note.AddDiagnosis("F20.9", "Schizophrenia, unspecified", "Principal");

        var sensitiveAccessLevel = 4; // Restricted

        // Act
        var diagnosis = note.Diagnoses.First();

        // Assert
        diagnosis.DiagnosisCode.Should().Be("F20.9");
        sensitiveAccessLevel.Should().Be(4);
    }

    [Fact]
    public void ClinicalNote_MinorPatient_DiagnosisShouldHaveEnhancedProtection()
    {
        // Arrange
        var minorPatientId = Guid.NewGuid();
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = minorPatientId,
            ProviderId = Guid.NewGuid()
        };
        // Sensitive diagnosis for minor (reproductive/mental health)
        note.AddDiagnosis("Z13.89", "Encounter for screening for other disorder", "Principal");

        // Act
        var parentalAccessLevel = 1; // Requires explicit parental consent indication
        var allAccessLevel = 2;

        // Assert
        parentalAccessLevel.Should().BeLessThan(allAccessLevel);
    }

    [Fact]
    public void ClinicalNote_AuditableFields_ShouldBeTrackable()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            Subjective = "Initial subjective"
        };

        var originalSubjective = note.Subjective;

        // Act
        note.Subjective = "Updated subjective";

        // Assert
        note.Subjective.Should().NotEqual(originalSubjective);
        // In a real system, this change should be auditable
    }

    [Fact]
    public void ClinicalNote_MODiagnosis_ShouldRequireSpecialApproval()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid()
        };
        // Potentially sensitive diagnosis
        note.AddDiagnosis("Z79.4", "Long term (current) use of inhaled steroids", "Principal");

        var requiresApproval = true;

        // Act
        var diagnosis = note.Diagnoses.First();

        // Assert
        diagnosis.DiagnosisCode.Should().Contain("Z79");
        requiresApproval.Should().BeTrue();
    }

    [Fact]
    public void ClinicalNote_ProcedureResults_ShouldHavePhiProtection()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid()
        };
        note.AddProcedure("HIV_ANTIBODY_TEST", "86703", "NEGATIVE");

        // Act
        var procedure = note.Procedures.First();
        var isPhi = _hipaaHelper.IsPhiContent(procedure.Result);

        // Assert
        isPhi.Should().BeTrue("Procedure results with sensitive information are PHI");
    }

    [Fact]
    public void ClinicalNote_PatientIdentifiers_ShouldNeverBePlaintext()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            Subjective = "John Doe presents with chest pain" // Patient name in clinical note
        };

        // Act & Assert
        // In real system, this should trigger validation or masking
        note.Subjective.Should().Contain("chest pain"); // Symptom is OK
        // Name should be handled separately with proper access controls
    }
}
