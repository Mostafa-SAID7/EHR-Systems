using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data;
using EHRPlatform.BuildingBlocks.EventBus.Messaging;
using EHRPlatform.Services.Clinical.Application.ClinicalNotes.Responses;
using EHRPlatform.Services.Clinical.Domain.Events;
using EHRPlatform.Services.Clinical.Features.ClinicalNotes.Commands;
using EHRPlatform.Services.Clinical.Domain.Entities;

namespace EHRPlatform.Services.Clinical.Features.ClinicalNotes.Handlers;

/// <summary>
/// Record vitals handler.
/// </summary>
public class RecordVitalsCommandHandler : ICommandHandler<RecordVitalsCommand, ClinicalNoteResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ILogger<RecordVitalsCommandHandler> _logger;

    public RecordVitalsCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ILogger<RecordVitalsCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task<ClinicalNoteResponse> Handle(RecordVitalsCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recording vitals for note {NoteId}", command.ClinicalNoteId);

        var repo = _unitOfWork.Repository<ClinicalNote>();
        var note = await repo.FirstOrDefaultAsync(
            q => q.Where(n => n.Id == command.ClinicalNoteId),
            cancellationToken);

        if (note == null)
            throw new InvalidOperationException($"Clinical note {command.ClinicalNoteId} not found");

        note.RecordVitals(
            command.Temperature,
            command.SystolicBP,
            command.DiastolicBP,
            command.HeartRate,
            command.RespiratoryRate,
            command.Weight);

        await repo.UpdateAsync(note, cancellationToken);

        // Publish event
        var vitalsEvent = note.GetDomainEvents().Last();
        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = note.Id,
            EventType = nameof(VitalSignsRecordedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(vitalsEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ClinicalNoteResponse { Id = note.Id, PatientId = note.PatientId };
    }
}


