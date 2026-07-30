using FluentAssertions;
using Moq;
using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Messaging;
using EHRPlatform.Services.Clinical.Domain.Entities;
using EHRPlatform.Services.Clinical.Domain.Events;
using EHRPlatform.Services.Clinical.Features.ClinicalNotes.Commands;
using EHRPlatform.Services.Clinical.Features.ClinicalNotes.Handlers;
using EHRPlatform.Services.Clinical.Application.ClinicalNotes.Mappers;
using EHRPlatform.Services.Clinical.Application.ClinicalNotes.Responses;
using EHRPlatform.Tests.Common.Builders;
using EHRPlatform.Tests.Common.Helpers;
using Microsoft.Extensions.Logging;
using Xunit;

namespace EHRPlatform.Tests.Unit.Services;

/// <summary>
/// Unit tests for Clinical Service command handlers.
/// Tests CQRS pattern, event publishing, and outbox pattern implementation.
/// HIPAA: All clinical note operations must be audited via outbox events.
/// </summary>
public class ClinicalNoteServiceTests
{
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly Mock<IRepository<ClinicalNote>> _mockRepository;
    private readonly Mock<IOutboxRepository> _mockOutbox;
    private readonly ClinicalNoteMapper _mapper;
    private readonly Mock<ILogger<CreateClinicalNoteCommandHandler>> _mockLogger;

    public ClinicalNoteServiceTests()
    {
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _mockRepository = new Mock<IRepository<ClinicalNote>>();
        _mockOutbox = new Mock<IOutboxRepository>();
        _mapper = new ClinicalNoteMapper();
        _mockLogger = new Mock<ILogger<CreateClinicalNoteCommandHandler>>();

        _mockUnitOfWork
            .Setup(u => u.Repository<ClinicalNote>())
            .Returns(_mockRepository.Object);
    }

    [Fact]
    public async Task CreateClinicalNoteCommandHandler_WithValidCommand_ShouldCreateNoteAndPublishEvent()
    {
        // Arrange
        var command = new CreateClinicalNoteCommand
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            EncounterDate = DateTime.UtcNow.AddDays(-1),
            EncounterType = "Office",
            Subjective = "Patient reports fatigue",
            Objective = "BP: 120/80",
            Assessment = "Anemia suspected",
            Plan = "Order CBC"
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<ClinicalNote>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockOutbox.Setup(o => o.AddAsync(It.IsAny<OutboxEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateClinicalNoteCommandHandler(_mockUnitOfWork.Object, _mockOutbox.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.PatientId.Should().Be(command.PatientId);
        result.ProviderId.Should().Be(command.ProviderId);
        result.Status.Should().Be("Draft");
        result.Subjective.Should().Be(command.Subjective);

        _mockRepository.Verify(r => r.AddAsync(It.IsAny<ClinicalNote>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockOutbox.Verify(o => o.AddAsync(It.IsAny<OutboxEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateClinicalNoteCommandHandler_ShouldPublishClinicalNoteCreatedEvent()
    {
        // Arrange
        var command = new CreateClinicalNoteCommand
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            EncounterDate = DateTime.UtcNow.AddDays(-1),
            EncounterType = "Office"
        };

        OutboxEvent? capturedEvent = null;
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<ClinicalNote>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockOutbox.Setup(o => o.AddAsync(It.IsAny<OutboxEvent>(), It.IsAny<CancellationToken>()))
            .Callback<OutboxEvent, CancellationToken>((evt, _) => capturedEvent = evt)
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateClinicalNoteCommandHandler(_mockUnitOfWork.Object, _mockOutbox.Object, _mapper, _mockLogger.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        capturedEvent.Should().NotBeNull();
        capturedEvent.EventType.Should().Be(nameof(ClinicalNoteCreatedEvent));
        capturedEvent.EventData.Should().Contain(command.PatientId.ToString());
    }

    [Fact]
    public async Task CreateClinicalNoteCommandHandler_WithMinimalData_ShouldCreateDraftNote()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var providerId = Guid.NewGuid();
        var command = new CreateClinicalNoteCommand
        {
            PatientId = patientId,
            ProviderId = providerId,
            EncounterDate = DateTime.UtcNow.AddDays(-1),
            EncounterType = "Telehealth"
        };

        _mockRepository.Setup(r => r.AddAsync(It.IsAny<ClinicalNote>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockOutbox.Setup(o => o.AddAsync(It.IsAny<OutboxEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateClinicalNoteCommandHandler(_mockUnitOfWork.Object, _mockOutbox.Object, _mapper, _mockLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Status.Should().Be("Draft");
        result.Subjective.Should().BeEmpty();
        result.Objective.Should().BeEmpty();
        result.Assessment.Should().BeEmpty();
        result.Plan.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateClinicalNoteCommandHandler_ShouldInitializeWithDraftStatus()
    {
        // Arrange
        var command = new CreateClinicalNoteCommand
        {
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            EncounterDate = DateTime.UtcNow.AddDays(-1),
            EncounterType = "Emergency"
        };

        ClinicalNote? capturedNote = null;
        _mockRepository.Setup(r => r.AddAsync(It.IsAny<ClinicalNote>(), It.IsAny<CancellationToken>()))
            .Callback<ClinicalNote, CancellationToken>((note, _) => capturedNote = note)
            .Returns(Task.CompletedTask);
        _mockOutbox.Setup(o => o.AddAsync(It.IsAny<OutboxEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new CreateClinicalNoteCommandHandler(_mockUnitOfWork.Object, _mockOutbox.Object, _mapper, _mockLogger.Object);

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        capturedNote.Should().NotBeNull();
        capturedNote!.Status.Should().Be("Draft");
        capturedNote.EncounterType.Should().Be("Emergency");
    }

    [Fact]
    public async Task AddDiagnosisCommandHandler_WithValidCommand_ShouldAddDiagnosis()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        var command = new AddDiagnosisCommand
        {
            ClinicalNoteId = noteId,
            DiagnosisCode = "I10",
            DiagnosisText = "Essential hypertension",
            DiagnosisType = "Principal"
        };

        var note = new ClinicalNote { Id = noteId, PatientId = Guid.NewGuid(), ProviderId = Guid.NewGuid() };
        _mockRepository.Setup(r => r.GetByIdAsync(noteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);
        _mockRepository.Setup(r => r.UpdateAsync(note, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockOutbox.Setup(o => o.AddAsync(It.IsAny<OutboxEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new AddDiagnosisCommandHandler(_mockUnitOfWork.Object, _mockOutbox.Object, _mockLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.GetByIdAsync(noteId, It.IsAny<CancellationToken>()), Times.Once);
        _mockRepository.Verify(r => r.UpdateAsync(note, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task FinalizeClinicalNoteCommandHandler_WithValidDraftNote_ShouldFinalize()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        var command = new FinalizeClinicalNoteCommand { ClinicalNoteId = noteId };

        var note = new ClinicalNote
        {
            Id = noteId,
            PatientId = Guid.NewGuid(),
            ProviderId = Guid.NewGuid(),
            Status = "Draft"
        };

        _mockRepository.Setup(r => r.GetByIdAsync(noteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);
        _mockRepository.Setup(r => r.UpdateAsync(note, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockOutbox.Setup(o => o.AddAsync(It.IsAny<OutboxEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new FinalizeClinicalNoteCommandHandler(_mockUnitOfWork.Object, _mockOutbox.Object, _mockLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be("Finalized");
        _mockOutbox.Verify(o => o.AddAsync(It.IsAny<OutboxEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RecordVitalsCommandHandler_WithValidVitals_ShouldRecordVitals()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        var command = new RecordVitalsCommand
        {
            ClinicalNoteId = noteId,
            Temperature = 98.6m,
            SystolicBP = 120,
            DiastolicBP = 80,
            HeartRate = 72,
            RespiratoryRate = 16
        };

        var note = new ClinicalNote { Id = noteId, PatientId = Guid.NewGuid(), ProviderId = Guid.NewGuid() };
        _mockRepository.Setup(r => r.GetByIdAsync(noteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);
        _mockRepository.Setup(r => r.UpdateAsync(note, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockOutbox.Setup(o => o.AddAsync(It.IsAny<OutboxEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new RecordVitalsCommandHandler(_mockUnitOfWork.Object, _mockOutbox.Object, _mockLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mockOutbox.Verify(o => o.AddAsync(It.IsAny<OutboxEvent>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddProcedureCommandHandler_WithValidProcedure_ShouldAddProcedure()
    {
        // Arrange
        var noteId = Guid.NewGuid();
        var command = new AddProcedureCommand
        {
            ClinicalNoteId = noteId,
            ProcedureName = "Chest X-Ray",
            ProcedureCode = "71046",
            Result = "Normal, no acute findings"
        };

        var note = new ClinicalNote { Id = noteId, PatientId = Guid.NewGuid(), ProviderId = Guid.NewGuid() };
        _mockRepository.Setup(r => r.GetByIdAsync(noteId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(note);
        _mockRepository.Setup(r => r.UpdateAsync(note, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockOutbox.Setup(o => o.AddAsync(It.IsAny<OutboxEvent>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _mockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new AddProcedureCommandHandler(_mockUnitOfWork.Object, _mockOutbox.Object, _mockLogger.Object);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _mockRepository.Verify(r => r.UpdateAsync(note, It.IsAny<CancellationToken>()), Times.Once);
    }
}

