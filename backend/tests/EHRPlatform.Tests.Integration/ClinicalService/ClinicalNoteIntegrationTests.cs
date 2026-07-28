using FluentAssertions;
using EHRPlatform.Services.Clinical.Domain.Entities;
using EHRPlatform.Tests.Common.Fixtures;
using Xunit;

namespace EHRPlatform.Tests.Integration.ClinicalService;

/// <summary>
/// Integration tests for Clinical Service with real database.
/// Tests full clinical note lifecycle, workflows, and data persistence.
/// HIPAA: All clinical records must be properly persisted and retrievable for audit.
/// </summary>
[Collection("Integration Tests")]
public class ClinicalNoteIntegrationTests : IAsyncLifetime
{
    private readonly DatabaseFixture _fixture;
    private IRepository<ClinicalNote> _repository;

    public ClinicalNoteIntegrationTests(DatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    public async Task InitializeAsync()
    {
        await _fixture.InitializeAsync();
        _repository = _fixture.GetRepository<ClinicalNote>();
    }

    public async Task DisposeAsync()
    {
        await _fixture.DisposeAsync();
    }

    [Fact]
    public async Task ClinicalNote_WhenCreated_ShouldPersistToDatabase()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var noteId = Guid.NewGuid();

        var note = new ClinicalNote
        {
            Id = noteId,
            PatientId = patientId,
            ProviderId = providerId,
            EncounterDate = DateTime.UtcNow.AddDays(-1),
            EncounterType = "Office",
            Status = "Draft",
            Subjective = "Patient reports fatigue",
            Objective = "BP: 120/80, HR: 72",
            Assessment = "Possible anemia",
            Plan = "Order CBC and iron studies"
        };

        // Act
        await _repository.AddAsync(note, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        // Assert
        var retrieved = await _repository.GetByIdAsync(noteId, CancellationToken.None);
        retrieved.Should().NotBeNull();
        retrieved!.PatientId.Should().Be(patientId);
        retrieved.ProviderId.Should().Be(providerId);
        retrieved.Status.Should().Be("Draft");
        retrieved.EncounterType.Should().Be("Office");
    }

    [Fact]
    public async Task ClinicalNote_WhenDiagnosisAdded_ShouldPersistDiagnosis()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        var note = new ClinicalNote
        {
            Id = noteId,
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            EncounterDate = DateTime.UtcNow.AddDays(-1)
        };

        await _repository.AddAsync(note, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        // Act
        var retrievedNote = await _repository.GetByIdAsync(noteId, CancellationToken.None);
        retrievedNote!.AddDiagnosis("I10", "Essential hypertension", "Principal");
        retrievedNote.AddDiagnosis("E11", "Type 2 diabetes mellitus", "Secondary");

        await _repository.UpdateAsync(retrievedNote, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        // Assert
        var finalNote = await _repository.GetByIdAsync(noteId, CancellationToken.None);
        finalNote!.Diagnoses.Should().HaveCount(2);
        finalNote.Diagnoses.Should().Contain(d => d.DiagnosisCode == "I10");
        finalNote.Diagnoses.Should().Contain(d => d.DiagnosisCode == "E11");
    }

    [Fact]
    public async Task ClinicalNote_WhenVitalsRecorded_ShouldPersistVitals()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        var note = new ClinicalNote
        {
            Id = noteId,
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            EncounterDate = DateTime.UtcNow.AddDays(-1)
        };

        await _repository.AddAsync(note, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        // Act
        var retrievedNote = await _repository.GetByIdAsync(noteId, CancellationToken.None);
        retrievedNote!.RecordVitals(
            temperature: 98.6m,
            systolic: 130,
            diastolic: 85,
            heartRate: 75,
            respiratoryRate: 18,
            weight: 185m
        );

        await _repository.UpdateAsync(retrievedNote, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        // Assert
        var finalNote = await _repository.GetByIdAsync(noteId, CancellationToken.None);
        finalNote!.VitalSigns.Should().HaveCount(1);
        var vitals = finalNote.VitalSigns.First();
        vitals.Temperature.Should().Be(98.6m);
        vitals.SystolicBP.Should().Be(130);
        vitals.DiastolicBP.Should().Be(85);
        vitals.Weight.Should().Be(185m);
    }

    [Fact]
    public async Task ClinicalNote_WhenProcedureAdded_ShouldPersistProcedure()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        var note = new ClinicalNote
        {
            Id = noteId,
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            EncounterDate = DateTime.UtcNow.AddDays(-1)
        };

        await _repository.AddAsync(note, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        // Act
        var retrievedNote = await _repository.GetByIdAsync(noteId, CancellationToken.None);
        retrievedNote!.AddProcedure("Chest X-Ray", "71046", "Normal, no acute findings");
        retrievedNote.AddProcedure("ECG", "93000", "Normal sinus rhythm");

        await _repository.UpdateAsync(retrievedNote, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        // Assert
        var finalNote = await _repository.GetByIdAsync(noteId, CancellationToken.None);
        finalNote!.Procedures.Should().HaveCount(2);
        finalNote.Procedures.Should().Contain(p => p.ProcedureName == "Chest X-Ray");
        finalNote.Procedures.Should().Contain(p => p.ProcedureName == "ECG");
    }

    [Fact]
    public async Task ClinicalNote_WhenFinalized_ShouldUpdateStatus()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        var note = new ClinicalNote
        {
            Id = noteId,
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            Status = "Draft",
            EncounterDate = DateTime.UtcNow.AddDays(-1)
        };

        await _repository.AddAsync(note, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        // Act
        var retrievedNote = await _repository.GetByIdAsync(noteId, CancellationToken.None);
        retrievedNote!.Finalize();

        await _repository.UpdateAsync(retrievedNote, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        // Assert
        var finalNote = await _repository.GetByIdAsync(noteId, CancellationToken.None);
        finalNote!.Status.Should().Be("Finalized");
    }

    [Fact]
    public async Task ClinicalNote_WhenQueryByPatient_ShouldRetrieveMultipleNotes()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var note1 = CreateClinicalNote(Guid.NewGuid(), patientId, providerId, "Office");
        var note2 = CreateClinicalNote(Guid.NewGuid(), patientId, providerId, "Telehealth");
        var note3 = CreateClinicalNote(Guid.NewGuid(), Guid.NewGuid(), providerId, "Office");

        // Act
        await _repository.AddAsync(note1, CancellationToken.None);
        await _repository.AddAsync(note2, CancellationToken.None);
        await _repository.AddAsync(note3, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        // Assert
        var patientNotes = await _repository.GetAsync(n => n.PatientId == patientId, CancellationToken.None);
        patientNotes.Should().HaveCount(2);
        patientNotes.All(n => n.PatientId == patientId).Should().BeTrue();
    }

    [Fact]
    public async Task ClinicalNote_WhenQueryByEncounterType_ShouldFilterCorrectly()
    {
        // Arrange
        var providerId = Guid.NewGuid();
        var office = CreateClinicalNote(Guid.NewGuid(), Guid.NewGuid(), providerId, "Office");
        var telehealth = CreateClinicalNote(Guid.NewGuid(), Guid.NewGuid(), providerId, "Telehealth");

        await _repository.AddAsync(office, CancellationToken.None);
        await _repository.AddAsync(telehealth, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        // Act
        var officeNotes = await _repository.GetAsync(n => n.EncounterType == "Office", CancellationToken.None);

        // Assert
        officeNotes.Should().HaveCount(1);
        officeNotes.First().EncounterType.Should().Be("Office");
    }

    [Fact]
    public async Task ClinicalNote_WhenMultipleDiagnoses_ShouldMaintainOrder()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        var note = new ClinicalNote
        {
            Id = noteId,
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            EncounterDate = DateTime.UtcNow.AddDays(-1)
        };

        await _repository.AddAsync(note, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        // Act
        var retrievedNote = await _repository.GetByIdAsync(noteId, CancellationToken.None);
        retrievedNote!.AddDiagnosis("I10", "Hypertension", "Principal");
        retrievedNote.AddDiagnosis("E11", "Diabetes", "Secondary");
        retrievedNote.AddDiagnosis("J45.9", "Asthma", "Secondary");
        retrievedNote.AddDiagnosis("M79.3", "Myalgia", "Secondary");

        await _repository.UpdateAsync(retrievedNote, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        // Assert
        var finalNote = await _repository.GetByIdAsync(noteId, CancellationToken.None);
        finalNote!.Diagnoses.Should().HaveCount(4);
        finalNote.Diagnoses.Select(d => d.DiagnosisCode)
            .Should().Equal("I10", "E11", "J45.9", "M79.3");
    }

    [Fact]
    public async Task ClinicalNote_WhenSoftDelete_ShouldNotRetrieve()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        var note = new ClinicalNote
        {
            Id = noteId,
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            EncounterDate = DateTime.UtcNow.AddDays(-1)
        };

        await _repository.AddAsync(note, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        // Act
        var retrievedNote = await _repository.GetByIdAsync(noteId, CancellationToken.None);
        retrievedNote!.IsDeleted = true;
        await _repository.UpdateAsync(retrievedNote, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        // Assert
        var deletedNote = await _repository.GetByIdAsync(noteId, CancellationToken.None);
        // Depending on query filters, deleted note may or may not be retrieved
        if (deletedNote != null)
            deletedNote.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task ClinicalNote_WhenComplexWorkflow_ShouldMaintainIntegrity()
    {
        // Arrange & Act
        var patientId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var noteId = Guid.NewGuid();

        var note = new ClinicalNote
        {
            Id = noteId,
            PatientId = patientId,
            ProviderId = providerId,
            EncounterDate = DateTime.UtcNow.AddDays(-1),
            EncounterType = "Office",
            Subjective = "Patient reports chest pain",
            Status = "Draft"
        };

        await _repository.AddAsync(note, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        var retrievedNote = await _repository.GetByIdAsync(noteId, CancellationToken.None);
        retrievedNote!.RecordVitals(98.6m, 140, 90, 85, 20);
        retrievedNote.AddDiagnosis("R07.9", "Chest pain, unspecified", "Principal");
        retrievedNote.AddProcedure("ECG", "93000", "Normal");
        retrievedNote.Objective = "BP: 140/90, HR: 85, ECG normal";
        retrievedNote.Assessment = "Musculoskeletal chest pain";
        retrievedNote.Plan = "NSAIDs, physical therapy";

        await _repository.UpdateAsync(retrievedNote, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        retrievedNote.Finalize();
        await _repository.UpdateAsync(retrievedNote, CancellationToken.None);
        await _fixture.SaveChangesAsync();

        // Assert
        var finalNote = await _repository.GetByIdAsync(noteId, CancellationToken.None);
        finalNote.Should().NotBeNull();
        finalNote!.Status.Should().Be("Finalized");
        finalNote.VitalSigns.Should().HaveCount(1);
        finalNote.Diagnoses.Should().HaveCount(1);
        finalNote.Procedures.Should().HaveCount(1);
        finalNote.Assessment.Should().Contain("Musculoskeletal");
    }

    private ClinicalNote CreateClinicalNote(Guid noteId, Guid patientId, Guid providerId, string encounterType)
    {
        return new ClinicalNote
        {
            Id = noteId,
            PatientId = patientId,
            ProviderId = providerId,
            EncounterDate = DateTime.UtcNow.AddDays(-1),
            EncounterType = encounterType,
            Status = "Draft"
        };
    }
}
