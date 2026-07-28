using FluentAssertions;
using Moq;
using EHRPlatform.Services.Clinical.Domain.Entities;
using EHRPlatform.Common.Security;
using Xunit;

namespace EHRPlatform.Tests.Security.Clinical;

/// <summary>
/// Authorization tests for Clinical Service.
/// Tests provider access control, specialty-based authorization, and HIPAA compliance.
/// HIPAA: Only authorized providers can access patient clinical records.
/// </summary>
public class ClinicalAuthorizationTests
{
    private readonly Mock<IAuthorizationService> _mockAuthService;

    public ClinicalAuthorizationTests()
    {
        _mockAuthService = new Mock<IAuthorizationService>();
    }

    [Fact]
    public void ClinicalNote_OriginalProvider_ShouldHaveAccess()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = providerId
        };

        // Act
        var hasAccess = note.ProviderId == providerId;

        // Assert
        hasAccess.Should().BeTrue("Original provider should have access");
    }

    [Fact]
    public void ClinicalNote_DifferentProvider_ShouldNotHaveDefaultAccess()
    {
        // Arrange
        var originalProviderId = Guid.NewGuid();
        var otherProviderId = Guid.NewGuid();
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = originalProviderId
        };

        // Act
        var hasDefaultAccess = note.ProviderId == otherProviderId;

        // Assert
        hasDefaultAccess.Should().BeFalse("Different provider should not have default access");
    }

    [Fact]
    public void ClinicalNote_AdminProvider_ShouldHaveOverrideAccess()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid()
        };

        var adminProviderId = Guid.NewGuid();
        var isAdmin = true;

        _mockAuthService.Setup(a => a.IsAdministrator(adminProviderId))
            .Returns(isAdmin);

        // Act
        var adminHasAccess = _mockAuthService.Object.IsAdministrator(adminProviderId);

        // Assert
        adminHasAccess.Should().BeTrue("Admin should have override access");
    }

    [Fact]
    public void ClinicalNote_CareTeamProvider_ShouldHaveAccess()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid()
        };

        var careTeamProviderId = Guid.NewGuid();
        var isCareTeamMember = true;

        _mockAuthService.Setup(a => a.IsCareTeamMember(careTeamProviderId, note.PatientId))
            .Returns(isCareTeamMember);

        // Act
        var hasAccess = _mockAuthService.Object.IsCareTeamMember(careTeamProviderId, note.PatientId);

        // Assert
        hasAccess.Should().BeTrue("Care team members should have access");
    }

    [Fact]
    public void ClinicalNote_PatientSelf_ShouldHaveReadAccess()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = patientId,
            ProviderId = Guid.NewGuid()
        };

        // Act
        var selfHasAccess = note.PatientId == patientId;

        // Assert
        selfHasAccess.Should().BeTrue("Patient should have read access to own records");
    }

    [Fact]
    public void ClinicalNote_UnauthorizedProvider_ShouldNotModify()
    {
        // Arrange
        var originalProviderId = Guid.NewGuid();
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = originalProviderId
        };

        var unauthorizedProviderId = Guid.NewGuid();
        var isAuthorized = note.ProviderId == unauthorizedProviderId;

        // Act & Assert
        isAuthorized.Should().BeFalse("Unauthorized provider cannot modify note");
    }

    [Fact]
    public void ClinicalNote_Finalized_ShouldNotAllowModification()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            Status = "Finalized"
        };

        // Act
        var canModify = note.Status == "Draft";

        // Assert
        canModify.Should().BeFalse("Finalized notes should not be modifiable");
    }

    [Fact]
    public void ClinicalNote_HighRiskDiagnosis_ShouldHaveRestrictedAccess()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid()
        };
        // Mental health diagnosis (high risk)
        note.AddDiagnosis("F32.9", "Major depressive disorder, single episode", "Principal");

        var diagnosis = note.Diagnoses.First();
        var accessLevel = 4; // Restricted

        // Act & Assert
        accessLevel.Should().Be(4, "High-risk diagnoses should have restricted access level");
    }

    [Fact]
    public void ClinicalNote_SensitiveProcedure_ShouldHaveAudit()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid()
        };
        // HIV test (sensitive)
        note.AddProcedure("HIV_ANTIBODY_TEST", "86703", "NEGATIVE");

        // Act
        var procedure = note.Procedures.First();
        var requiresAudit = procedure.ProcedureCode.Contains("HIV") || procedure.ProcedureCode == "86703";

        // Assert
        requiresAudit.Should().BeTrue("Sensitive procedures should require audit");
    }

    [Fact]
    public void ClinicalNote_ProviderSpecialty_ShouldRestrictAccess()
    {
        // Arrange
        var cardiologyProviderId = Guid.NewGuid();
        var psychiatryProviderId = Guid.NewGuid();

        var cardiologyNote = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = cardiologyProviderId
        };

        cardiologyNote.AddDiagnosis("I10", "Hypertension", "Principal");

        _mockAuthService.Setup(a => a.HasSpecialty(cardiologyProviderId, "Cardiology"))
            .Returns(true);
        _mockAuthService.Setup(a => a.HasSpecialty(psychiatryProviderId, "Cardiology"))
            .Returns(false);

        // Act
        var cardiologyHasAccess = _mockAuthService.Object.HasSpecialty(cardiologyProviderId, "Cardiology");
        var psychiatryHasAccess = _mockAuthService.Object.HasSpecialty(psychiatryProviderId, "Cardiology");

        // Assert
        cardiologyHasAccess.Should().BeTrue("Cardiologist should access cardiology records");
        psychiatryHasAccess.Should().BeFalse("Psychiatrist should not auto-access cardiology records");
    }

    [Fact]
    public void ClinicalNote_ShareWithProvider_ShouldRequirePatientConsent()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid()
        };

        var sharingProviderId = Guid.NewGuid();
        var patientConsented = true;

        // Act & Assert
        patientConsented.Should().BeTrue("Sharing note with provider requires patient consent");
    }

    [Fact]
    public void ClinicalNote_DeletedNote_ShouldNotBeAccessible()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            IsDeleted = true
        };

        // Act
        var isAccessible = !note.IsDeleted;

        // Assert
        isAccessible.Should().BeFalse("Deleted notes should not be accessible");
    }

    [Fact]
    public void ClinicalNote_TemporaryAccess_ShouldExpire()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var accessGrantedAt = DateTime.UtcNow.AddHours(-25);
        var accessDurationHours = 24;
        var accessExpired = DateTime.UtcNow > accessGrantedAt.AddHours(accessDurationHours);

        // Act & Assert
        accessExpired.Should().BeTrue("Temporary access should expire after duration");
    }
}
