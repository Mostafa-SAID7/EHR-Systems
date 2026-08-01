using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.BuildingBlocks.Common.Messaging;
using EHRPlatform.Services.Clinical.Features.ClinicalNotes.Commands;
using EHRPlatform.Services.Clinical.Application.ClinicalNotes.Responses;
using EHRPlatform.Services.Clinical.Application.ClinicalNotes.Mappers;
using EHRPlatform.Services.Clinical.Domain.Events;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Clinical.Features.ClinicalNotes.Handlers;

/// <summary>
/// Create clinical note command handler.
/// Creates new clinical note in draft status.
/// </summary>
public class CreateClinicalNoteCommandHandler : ICommandHandler<CreateClinicalNoteCommand, ClinicalNoteResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ClinicalNoteMapper _mapper;
    private readonly ILogger<CreateClinicalNoteCommandHandler> _logger;

    public CreateClinicalNoteCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ClinicalNoteMapper mapper,
        ILogger<CreateClinicalNoteCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger;
    }

    public async Task<ClinicalNoteResponse> Handle(CreateClinicalNoteCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating clinical note for patient {PatientId}", command.PatientId);

        var note = new Domain.Entities.ClinicalNote
        {
            Id = Guid.NewGuid(),
            PatientId = command.PatientId,
            ProviderId = command.ProviderId,
            EncounterDate = command.EncounterDate,
            EncounterType = command.EncounterType,
            Subjective = command.Subjective ?? "",
            Objective = command.Objective ?? "",
            Assessment = command.Assessment ?? "",
            Plan = command.Plan ?? "",
            Status = "Draft"
        };

        var repo = _unitOfWork.Repository<Domain.Entities.ClinicalNote>();
        await repo.AddAsync(note, cancellationToken);

        // Publish event
        var createdEvent = new ClinicalNoteCreatedEvent(
            note.Id, note.PatientId, note.ProviderId, note.EncounterDate, note.EncounterType);

        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = note.Id,
            EventType = nameof(ClinicalNoteCreatedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(createdEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Clinical note created {NoteId}", note.Id);

        return _mapper.MapToResponse(note);
    }
}


