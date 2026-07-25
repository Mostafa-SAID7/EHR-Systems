using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Messaging;
using EHRPlatform.Services.Clinical.Features.ClinicalNotes.Commands;
using EHRPlatform.Services.Clinical.Application.ClinicalNotes.Responses;
using EHRPlatform.Services.Clinical.Application.ClinicalNotes.Mappers;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Clinical.Features.ClinicalNotes.Handlers;

/// <summary>
/// Finalize clinical note command handler.
/// Moves note from draft to finalized status.
/// </summary>
public class FinalizeClinicalNoteCommandHandler : ICommandHandler<FinalizeClinicalNoteCommand, ClinicalNoteResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ClinicalNoteMapper _mapper;
    private readonly ILogger<FinalizeClinicalNoteCommandHandler> _logger;

    public FinalizeClinicalNoteCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ClinicalNoteMapper mapper,
        ILogger<FinalizeClinicalNoteCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger;
    }

    public async Task<ClinicalNoteResponse> Handle(FinalizeClinicalNoteCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finalizing clinical note {NoteId}", command.ClinicalNoteId);

        var repo = _unitOfWork.Repository<Domain.Entities.ClinicalNote>();
        var note = await repo.GetByIdAsync(command.ClinicalNoteId, cancellationToken);

        if (note == null)
            throw new KeyNotFoundException($"Clinical note {command.ClinicalNoteId} not found");

        note.Finalize();

        await repo.UpdateAsync(note, cancellationToken);

        // Publish event
        var finalizedEvent = new ClinicalNoteCompletedEvent(
            note.Id, note.PatientId, note.EncounterDate);

        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = note.Id,
            EventType = nameof(ClinicalNoteCompletedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(finalizedEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Clinical note finalized {NoteId}", command.ClinicalNoteId);

        return _mapper.MapToResponse(note);
    }
}
