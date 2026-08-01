using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Commands;

/// <summary>
/// Add diagnosis handler.
/// Adds ICD-10 diagnosis to clinical note.
/// Publishes DiagnosisRecordedEvent for subscriber services.
/// </summary>
public class AddDiagnosisCommandHandler : ICommandHandler<AddDiagnosisCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ILogger<AddDiagnosisCommandHandler> _logger;

    public AddDiagnosisCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ILogger<AddDiagnosisCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        AddDiagnosisCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Adding diagnosis to clinical note {ClinicalNoteId}: {ICD10Code}",
            command.ClinicalNoteId, command.ICD10Code);

        if (string.IsNullOrEmpty(command.ICD10Code))
            throw new ArgumentException("ICD10Code cannot be empty");

        var repository = _unitOfWork.Repository<Domain.ClinicalNote>();
        var note = await repository.GetByIdAsync(command.ClinicalNoteId, cancellationToken);

        if (note == null)
            throw new KeyNotFoundException($"Clinical note {command.ClinicalNoteId} not found");

        // Validate ICD-10 format (basic validation)
        if (!System.Text.RegularExpressions.Regex.IsMatch(command.ICD10Code, @"^[A-Z]\d{2}(\.\d{1,2})?$"))
            throw new ArgumentException($"Invalid ICD-10 code format: {command.ICD10Code}");

        var diagnosis = new Domain.Diagnosis
        {
            Id = Guid.NewGuid(),
            ClinicalNoteId = note.Id,
            ICD10Code = command.ICD10Code,
            Description = command.Description ?? string.Empty,
            RecordedAt = DateTime.UtcNow
        };

        note.AddDiagnosis(diagnosis);
        note.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(note, cancellationToken);

        // Publish event
        var diagnosisEvent = new DiagnosisRecordedEvent(
            note.Id,
            note.PatientId,
            command.ICD10Code,
            command.Description ?? string.Empty,
            DateTime.UtcNow);

        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = note.Id,
            EventType = nameof(DiagnosisRecordedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(diagnosisEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Diagnosis added to clinical note {ClinicalNoteId}", note.Id);

        return Unit.Value;
    }
}

/// <summary>
/// Domain event: Diagnosis recorded
/// </summary>
public record DiagnosisRecordedEvent(Guid ClinicalNoteId, Guid PatientId, string ICD10Code, string Description, DateTime RecordedAt);
