using FluentAssertions;
using EHRPlatform.Services.Clinical.Domain.Entities;
using EHRPlatform.Services.Clinical.Domain.Events;
using Xunit;

namespace EHRPlatform.Tests.Unit.Domain;

/// <summary>
/// Unit tests for ClinicalNote aggregate.
/// Tests SOAP note lifecycle, domain events, and state transitions.
/// HIPAA: Clinical notes are protected health information (PHI) requiring audit trail.
/// </summary>
public class ClinicalNoteTests
{
    private readonly Guid _patientId = Guid.NewGuid();
    private readonly Guid _providerId = Guid.NewGuid();
    private readonly DateTime _encounterDate = DateTime.UtcNow.AddDays(-1);

    [Fact]
    public void ClinicalNote_WhenCreated_ShouldInitializeWithDraftStatus()
    {
        // Arrange
        var noteId = Guid.NewGuid();

        // Act
        var note = new ClinicalNote
        {
            Id = noteId,
            PatientId = _patientId,
            ProviderId = _providerId,
            EncounterDate = _encounterDate,
            EncounterType = "Office",
            Status = "Draft"
        };

        // Assert
        note.Status.Should().Be("Draft");
        note.PatientId.Should().Be(_patientId);
        note.ProviderId.Should().Be(_providerId);
        note.EncounterDate.Should().Be(_encounterDate);
        note.EncounterType.Should().Be("Office");
    }

    [Fact]
    public void ClinicalNote_WhenFinalized_ShouldUpdateStatusAndRaiseEvent()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = _patientId,
            ProviderId = _providerId,
            EncounterDate = _encounterDate,
            Status = "Draft"
        };

        // Act
        note.Finalize();

        // Assert
        note.Status.Should().Be("Finalized");
        note.GetDomainEvents().Should().ContainSingle(e => e is ClinicalNoteCompletedEvent);
    }

    [Fact]
    public void ClinicalNote_WhenFinalized_ShouldThrowIfNotDraft()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = _patientId,
            ProviderId = _providerId,
            Status = "Finalized"
        };

        // Act & Assert
        var action = () => note.Finalize();
        action.Should().Throw<InvalidOperationException>()
            .WithMessage("Only draft notes can be finalized");
    }

    [Fact]
    public void ClinicalNote_WhenAddDiagnosis_ShouldAddToDiagnosisCollection()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = _patientId,
            ProviderId = _providerId
        };

        // Act
        note.AddDiagnosis("I10", "Essential (primary) hypertension", "Principal");

        // Assert
        note.Diagnoses.Should().HaveCount(1);
        note.Diagnoses.First().DiagnosisCode.Should().Be("I10");
        note.Diagnoses.First().DiagnosisText.Should().Be("Essential (primary) hypertension");
        note.Diagnoses.First().DiagnosisType.Should().Be("Principal");
    }

    [Fact]
    public void ClinicalNote_WhenAddDiagnosis_ShouldRaiseEvent()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = _patientId,
            ProviderId = _providerId
        };

        // Act
        note.AddDiagnosis("E11", "Type 2 diabetes mellitus", "Secondary");

        // Assert
        note.GetDomainEvents().Should().ContainSingle(e => e is DiagnosisRecordedEvent);
        var @event = (DiagnosisRecordedEvent)note.GetDomainEvents().First();
        @event.DiagnosisCode.Should().Be("E11");
    }

    [Fact]
    public void ClinicalNote_WhenAddMultipleDiagnoses_ShouldMaintainCollection()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = _patientId,
            ProviderId = _providerId
        };

        // Act
        note.AddDiagnosis("I10", "Hypertension", "Principal");
        note.AddDiagnosis("E11", "Diabetes", "Secondary");
        note.AddDiagnosis("J45.9", "Asthma, unspecified", "Secondary");

        // Assert
        note.Diagnoses.Should().HaveCount(3);
        note.Diagnoses.Select(d => d.DiagnosisCode)
            .Should().Equal("I10", "E11", "J45.9");
    }

    [Fact]
    public void ClinicalNote_WhenRecordVitals_ShouldAddToVitalSignsCollection()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = _patientId,
            ProviderId = _providerId
        };

        // Act
        note.RecordVitals(
            temperature: 98.6m,
            systolic: 120,
            diastolic: 80,
            heartRate: 72,
            respiratoryRate: 16,
            weight: 175m
        );

        // Assert
        note.VitalSigns.Should().HaveCount(1);
        var vitals = note.VitalSigns.First();
        vitals.Temperature.Should().Be(98.6m);
        vitals.SystolicBP.Should().Be(120);
        vitals.DiastolicBP.Should().Be(80);
        vitals.HeartRate.Should().Be(72);
        vitals.RespiratoryRate.Should().Be(16);
        vitals.Weight.Should().Be(175m);
    }

    [Fact]
    public void ClinicalNote_WhenRecordVitals_ShouldRaiseEvent()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = _patientId,
            ProviderId = _providerId
        };

        // Act
        note.RecordVitals(
            temperature: 98.6m,
            systolic: 130,
            diastolic: 85,
            heartRate: 75,
            respiratoryRate: 18
        );

        // Assert
        note.GetDomainEvents().Should().ContainSingle(e => e is VitalSignsRecordedEvent);
        var @event = (VitalSignsRecordedEvent)note.GetDomainEvents().First();
        @event.SystolicBP.Should().Be(130);
    }

    [Fact]
    public void ClinicalNote_WhenAddProcedure_ShouldAddToProceduresCollection()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = _patientId,
            ProviderId = _providerId
        };

        // Act
        note.AddProcedure("Chest X-Ray", "71046", "Normal, no acute findings");

        // Assert
        note.Procedures.Should().HaveCount(1);
        note.Procedures.First().ProcedureName.Should().Be("Chest X-Ray");
        note.Procedures.First().ProcedureCode.Should().Be("71046");
        note.Procedures.First().Result.Should().Be("Normal, no acute findings");
    }

    [Fact]
    public void ClinicalNote_WhenAddProcedure_ShouldRaiseEvent()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = _patientId,
            ProviderId = _providerId
        };

        // Act
        note.AddProcedure("ECG", "93000", "Normal sinus rhythm");

        // Assert
        note.GetDomainEvents().Should().ContainSingle(e => e is ProcedurePerformedEvent);
        var @event = (ProcedurePerformedEvent)note.GetDomainEvents().First();
        @event.ProcedureName.Should().Be("ECG");
    }

    [Fact]
    public void ClinicalNote_WhenClearDomainEvents_ShouldRemoveAllEvents()
    {
        // Arrange
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = _patientId,
            ProviderId = _providerId
        };
        note.AddDiagnosis("I10", "Hypertension", "Principal");
        note.RecordVitals(98.6m, 120, 80, 72, 16);

        // Act
        note.ClearDomainEvents();

        // Assert
        note.GetDomainEvents().Should().BeEmpty();
    }

    [Fact]
    public void ClinicalNote_WhenSOAPComponentsProvided_ShouldInitializeAllComponents()
    {
        // Arrange
        var subjective = "Patient reports chest pain";
        var objective = "BP: 140/90, HR: 85";
        var assessment = "Suspected angina";
        var plan = "Order ECG and troponin levels";

        // Act
        var note = new ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = _patientId,
            ProviderId = _providerId,
            Subjective = subjective,
            Objective = objective,
            Assessment = assessment,
            Plan = plan
        };

        // Assert
        note.Subjective.Should().Be(subjective);
        note.Objective.Should().Be(objective);
        note.Assessment.Should().Be(assessment);
        note.Plan.Should().Be(plan);
    }
}
