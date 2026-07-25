using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Messaging;
using EHRPlatform.Services.Clinical.Features.ClinicalNotes.Commands;
using EHRPlatform.Services.Clinical.Application.ClinicalNotes.Responses;
using EHRPlatform.Services.Clinical.Application.ClinicalNotes.Mappers;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Clinical.Features.ClinicalNotes.Handlers;

/// <summary>
/// Record vital signs command handler.
/// Records vital signs for clinical note.
/// </summary>
public class RecordVitalSignsCommandHandler : ICommandHandler<RecordVitalsCommand, ClinicalNoteResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ClinicalNoteMapper _mapper;
    private readonly ILogger<RecordVitalSignsCommandHandler> _logger;

    public RecordVitalSignsCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ClinicalNoteMapper mapper,
        ILogger<RecordVitalSignsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger;
    }

    public async Task<ClinicalNoteResponse> Handle(RecordVitalsCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recording vital signs for clinical note {NoteId}", command.ClinicalNoteId);

        var repo = _unitOfWork.Repository<Domain.Entities.ClinicalNote>();
        var note = await repo.GetByIdAsync(command.ClinicalNoteId, cancellationToken);

        if (note == null)
            throw new KeyNotFoundException($"Clinical note {command.ClinicalNoteId} not found");

        note.RecordVitals(
            command.Temperature,
            command.SystolicBP,
            command.DiastolicBP,
            command.HeartRate,
            command.RespiratoryRate,
            command.Weight
        );

        await repo.UpdateAsync(note, cancellationToken);

        // Publish event
        var vitalsEvent = new VitalSignsRecordedEvent(
            note.Id, note.PatientId, command.SystolicBP, command.DiastolicBP, command.HeartRate);

        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = note.Id,
            EventType = nameof(VitalSignsRecordedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(vitalsEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Vital signs recorded for note {NoteId}", command.ClinicalNoteId);

        return _mapper.MapToResponse(note);
    }
}
