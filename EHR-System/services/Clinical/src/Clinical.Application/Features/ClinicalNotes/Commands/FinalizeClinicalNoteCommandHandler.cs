using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Commands;

/// <summary>
/// Finalize clinical note handler.
/// Locks note for editing (finalized status).
/// Publishes ClinicalNoteCompletedEvent for subscriber services.
/// </summary>
public class FinalizeClinicalNoteCommandHandler : ICommandHandler<FinalizeClinicalNoteCommand, Unit>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ILogger<FinalizeClinicalNoteCommandHandler> _logger;

    public FinalizeClinicalNoteCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ILogger<FinalizeClinicalNoteCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task<Unit> Handle(
        FinalizeClinicalNoteCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Finalizing clinical note {ClinicalNoteId}",
            command.ClinicalNoteId);

        var repository = _unitOfWork.Repository<Domain.ClinicalNote>();
        var note = await repository.GetByIdAsync(command.ClinicalNoteId, cancellationToken);

        if (note == null)
            throw new KeyNotFoundException($"Clinical note {command.ClinicalNoteId} not found");

        if (note.Status == "Finalized")
            throw new InvalidOperationException("Clinical note is already finalized");

        // Validate note has required SOAP components
        if (string.IsNullOrEmpty(note.Assessment) || string.IsNullOrEmpty(note.Plan))
            throw new InvalidOperationException("Assessment and Plan are required to finalize a note");

        note.Finalize();
        note.UpdatedAt = DateTime.UtcNow;

        await repository.UpdateAsync(note, cancellationToken);

        // Publish event
        var completedEvent = new ClinicalNoteCompletedEvent(
            note.Id,
            note.PatientId,
            note.ProviderId,
            note.Status,
            DateTime.UtcNow);

        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = note.Id,
            EventType = nameof(ClinicalNoteCompletedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(completedEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Clinical note finalized {ClinicalNoteId}", note.Id);

        return Unit.Value;
    }
}

/// <summary>
/// Domain event: Clinical note completed
/// </summary>
public record ClinicalNoteCompletedEvent(Guid ClinicalNoteId, Guid PatientId, Guid ProviderId, string Status, DateTime CompletedAt);
