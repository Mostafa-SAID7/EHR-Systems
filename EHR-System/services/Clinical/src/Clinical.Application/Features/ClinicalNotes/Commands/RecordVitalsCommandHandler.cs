using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Commands;

/// <summary>
/// Record vital signs handler.
/// Adds vital sign to clinical note.
/// Publishes VitalSignsRecordedEvent for subscriber services (e.g., Analytics).
/// </summary>
public class RecordVitalsCommandHandler : ICommandHandler<RecordVitalsCommand, Unit>
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

    public async Task<Unit> Handle(
        RecordVitalsCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Recording vitals for clinical note {ClinicalNoteId}: {VitalType} = {Value}",
            command.ClinicalNoteId, command.VitalType, command.Value);

        var repository = _unitOfWork.Repository<Domain.ClinicalNote>();
        var note = await repository.GetByIdAsync(command.ClinicalNoteId, cancellationToken);

        if (note == null)
            throw new KeyNotFoundException($"Clinical note {command.ClinicalNoteId} not found");

        // Validate vital type
        var validVitals = new[] { "BloodPressure", "Temperature", "Pulse", "RespirationRate", "OxygenSaturation", "Weight", "Height" };
        if (!validVitals.Contains(command.VitalType))
            throw new ArgumentException($"Invalid vital type: {command.VitalType}");

        var vital = new Domain.VitalSign
        {
            Id = Guid.NewGuid(),
            ClinicalNoteId = note.Id,
            VitalType = command.VitalType,
            Value = command.Value,
            Unit = command.Unit,
            RecordedAt = DateTime.UtcNow
        };

        note.AddVitalSign(vital);
        note.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(note, cancellationToken);

        // Publish event
        var vitalsEvent = new VitalSignsRecordedEvent(
            note.Id,
            note.PatientId,
            command.VitalType,
            command.Value,
            DateTime.UtcNow);

        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = note.Id,
            EventType = nameof(VitalSignsRecordedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(vitalsEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Vital signs recorded for clinical note {ClinicalNoteId}", note.Id);

        return Unit.Value;
    }
}

/// <summary>
/// Domain event: Vital signs recorded
/// </summary>
public record VitalSignsRecordedEvent(Guid ClinicalNoteId, Guid PatientId, string VitalType, string Value, DateTime RecordedAt);
