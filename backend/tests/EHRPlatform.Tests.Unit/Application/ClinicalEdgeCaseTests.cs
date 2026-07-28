using FluentAssertions;
using EHRPlatform.Services.Clinical.Domain.Entities;
using Xunit;

namespace EHRPlatform.Tests.Unit.Application;

/// <summary>
/// Edge case tests for Clinical Service.
/// Tests boundary conditions, unusual scenarios, and error handling.
/// HIPAA: Edge cases must not compromise data integrity or compliance.
/// </summary>
public class ClinicalEdgeCaseTests
{
    [Fact]
    public void ClinicalNote_WithMaxDiagnoses_ShouldHandleMany()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid()
        };

        // Act - Add 100 diagnoses
        for (int i = 0; i < 100; i++)
        {
            var code = $"A{i:D2}"; // Synthetic code
            note.AddDiagnosis("I10", $"Diagnosis {i}", i % 2 == 0 ? "Principal" : "Secondary");
        }

        // Assert
        note.Diagnoses.Should().HaveCount(100);
    }

    [Fact]
    public void ClinicalNote_WithVeryLongSOAPText_ShouldAccommodate()
    {
        // Arrange
        var longText = new string('X', 50000); // 50KB of text

        // Act
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            Subjective = longText,
            Objective = longText,
            Assessment = longText,
            Plan = longText
        };

        // Assert
        note.Subjective.Length.Should().Be(50000);
        note.Objective.Length.Should().Be(50000);
        note.Assessment.Length.Should().Be(50000);
        note.Plan.Length.Should().Be(50000);
    }

    [Fact]
    public void ClinicalNote_WithSpecialCharactersInText_ShouldPreserve()
    {
        // Arrange
        var specialText = "Patient reports: fever (101.5°F), chest pain ±, allergies: NSAIDs, β-blockers (contraindicated), vitals: BP 140/90 mmHg";

        // Act
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            Subjective = specialText
        };

        // Assert
        note.Subjective.Should().Equal(specialText);
        note.Subjective.Should().Contain("°F");
        note.Subjective.Should().Contain("β");
        note.Subjective.Should().Contain("mmHg");
    }

    [Fact]
    public void ClinicalNote_WithUtf8Characters_ShouldHandle()
    {
        // Arrange
        var utf8Text = "Résumé of 中文 患者 with العربية اللغة symptoms";

        // Act
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            Subjective = utf8Text
        };

        // Assert
        note.Subjective.Should().Equal(utf8Text);
    }

    [Fact]
    public void ClinicalNote_WithIdenticalDiagnosesDifferentTypes_ShouldAllowMultiple()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid()
        };

        // Act
        note.AddDiagnosis("I10", "Hypertension", "Principal");
        note.AddDiagnosis("I10", "Hypertension", "Secondary"); // Same code, different type

        // Assert
        note.Diagnoses.Should().HaveCount(2);
        note.Diagnoses.Count(d => d.DiagnosisCode == "I10").Should().Be(2);
    }

    [Fact]
    public void ClinicalNote_WithExtremeVitalValues_ShouldRecord()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid()
        };

        // Act - Record extreme but survivable vitals
        note.RecordVitals(
            temperature: 95.0m,  // Hypothermia
            systolic: 180,       // Severe hypertension
            diastolic: 120,
            heartRate: 180,      // Extreme tachycardia
            respiratoryRate: 50, // Extreme tachypnea
            weight: 50m          // Very low weight
        );

        // Assert
        note.VitalSigns.Should().HaveCount(1);
        var vitals = note.VitalSigns.First();
        vitals.Temperature.Should().Be(95.0m);
        vitals.SystolicBP.Should().Be(180);
        vitals.HeartRate.Should().Be(180);
    }

    [Fact]
    public void ClinicalNote_EncounterDateInFarPast_ShouldAccept()
    {
        // Arrange
        var farPastDate = DateTime.UtcNow.AddYears(-5);

        // Act
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            EncounterDate = farPastDate
        };

        // Assert
        note.EncounterDate.Should().Equal(farPastDate);
    }

    [Fact]
    public void ClinicalNote_EncounterDateToday_ShouldAccept()
    {
        // Arrange
        var today = DateTime.UtcNow;

        // Act
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            EncounterDate = today
        };

        // Assert
        note.EncounterDate.Date.Should().Equal(today.Date);
    }

    [Fact]
    public void ClinicalNote_MultipleProviderIds_ShouldNotCrossContaminate()
    {
        // Arrange
        var provider1 = Guid.NewGuid();
        var provider2 = Guid.NewGuid();

        var note1 = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = provider1
        };

        var note2 = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = provider2
        };

        // Act & Assert
        note1.ProviderId.Should().NotEqual(note2.ProviderId);
        note1.ProviderId.Should().Equal(provider1);
        note2.ProviderId.Should().Equal(provider2);
    }

    [Fact]
    public void ClinicalNote_WithOnlyPrincipalDiagnosis_ShouldBeValid()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid()
        };

        // Act
        note.AddDiagnosis("I10", "Hypertension", "Principal");

        // Assert
        note.Diagnoses.Should().HaveCount(1);
        note.Diagnoses.First().DiagnosisType.Should().Be("Principal");
    }

    [Fact]
    public void ClinicalNote_WithOnlySecondaryDiagnoses_ShouldBeValid()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid()
        };

        // Act
        note.AddDiagnosis("E11", "Diabetes", "Secondary");
        note.AddDiagnosis("J45.9", "Asthma", "Secondary");

        // Assert
        note.Diagnoses.Should().HaveCount(2);
        note.Diagnoses.All(d => d.DiagnosisType == "Secondary").Should().BeTrue();
    }

    [Fact]
    public void ClinicalNote_WithNoProcedures_ShouldBeValid()
    {
        // Arrange & Act
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid()
        };

        // Assert
        note.Procedures.Should().BeEmpty();
    }

    [Fact]
    public void ClinicalNote_EventsAfterClear_ShouldBeEmpty()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid()
        };

        note.AddDiagnosis("I10", "Test", "Principal");
        note.GetDomainEvents().Should().HaveCount(1);

        // Act
        note.ClearDomainEvents();

        // Assert
        note.GetDomainEvents().Should().BeEmpty();
    }

    [Fact]
    public void ClinicalNote_GuidsAreUnique()
    {
        // Arrange & Act
        var note1 = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid()
        };

        var note2 = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid()
        };

        // Assert
        note1.Id.Should().NotEqual(note2.Id);
        note1.PatientId.Should().NotEqual(note2.PatientId);
        note1.ProviderId.Should().NotEqual(note2.ProviderId);
    }
}
