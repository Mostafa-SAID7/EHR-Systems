using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Messaging;
using EHRPlatform.Services.Clinical.Features.ClinicalNotes.Commands;
using EHRPlatform.Services.Clinical.Application.ClinicalNotes.Responses;
using EHRPlatform.Services.Clinical.Application.ClinicalNotes.Mappers;
using EHRPlatform.Services.Clinical.Domain.Events;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Clinical.Features.ClinicalNotes.Handlers;

/// <summary>
/// Add diagnosis command handler.
/// Records diagnosis for clinical note.
/// </summary>
public class AddDiagnosisCommandHandler : ICommandHandler<AddDiagnosisCommand, ClinicalNoteResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ClinicalNoteMapper _mapper;
    private readonly ILogger<AddDiagnosisCommandHandler> _logger;

    public AddDiagnosisCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ClinicalNoteMapper mapper,
        ILogger<AddDiagnosisCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger;
    }

    public async Task<ClinicalNoteResponse> Handle(AddDiagnosisCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding diagnosis to clinical note {NoteId}", command.ClinicalNoteId);

        var repo = _unitOfWork.Repository<Domain.Entities.ClinicalNote>();
        var note = await repo.GetByIdAsync(command.ClinicalNoteId, cancellationToken);

        if (note == null)
            throw new KeyNotFoundException($"Clinical note {command.ClinicalNoteId} not found");

        note.AddDiagnosis(command.DiagnosisCode, command.DiagnosisText, command.DiagnosisType);

        await repo.UpdateAsync(note, cancellationToken);

        // Publish event
        var diagnosisEvent = new DiagnosisRecordedEvent(
            note.Id, note.PatientId, command.DiagnosisCode, command.DiagnosisText);

        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = note.Id,
            EventType = nameof(DiagnosisRecordedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(diagnosisEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Diagnosis added to note {NoteId}", note.Id);

        return _mapper.MapToResponse(note);
    }
}
