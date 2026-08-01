using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Clinical.Domain.Entities;
using EHRPlatform.Services.Clinical.Domain.Events;
using EHRPlatform.Services.Clinical.Application.Mappers;
using EHRPlatform.Services.Clinical.Application.Services;
using EHRPlatform.Services.Clinical.Contracts.Responses;
using EHRPlatform.Services.Clinical.Persistence;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Commands;

/// <summary>
/// Handles CreateClinicalNoteCommand.
/// Creates SOAP clinical note in draft status and publishes ClinicalNoteCreatedEvent.
/// </summary>
public class CreateClinicalNoteCommandHandler : IRequestHandler<CreateClinicalNoteCommand, ClinicalNoteResponse>
{
    private readonly ClinicalContext _context;
    private readonly ClinicalCacheService _cacheService;
    private readonly ClinicalDtoMapper _mapper;
    private readonly ILogger<CreateClinicalNoteCommandHandler> _logger;

    public CreateClinicalNoteCommandHandler(
        ClinicalContext context,
        ClinicalCacheService cacheService,
        ClinicalDtoMapper mapper,
        ILogger<CreateClinicalNoteCommandHandler> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ClinicalNoteResponse> Handle(
        CreateClinicalNoteCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Creating clinical note for Patient {PatientId} by Provider {ProviderId}",
            command.PatientId, command.ProviderId);

        var note = new ClinicalNote
        {
            Id           = Guid.NewGuid(),
            PatientId    = command.PatientId,
            ProviderId   = command.ProviderId,
            EncounterDate = command.EncounterDate,
            EncounterType = command.EncounterType,
            Status       = "Draft",
            Subjective   = command.Subjective ?? string.Empty,
            Objective    = command.Objective  ?? string.Empty,
            Assessment   = command.Assessment ?? string.Empty,
            Plan         = command.Plan       ?? string.Empty,
            CreatedAt    = DateTime.UtcNow,
            CreatedBy    = "system"
        };

        // Raise domain event
        note.RaiseEvent(new ClinicalNoteCreatedEvent(
            note.Id, note.PatientId, note.ProviderId, note.EncounterDate));

        _context.ClinicalNotes.Add(note);
        await _context.SaveChangesAsync(cancellationToken);

        // Populate cache
        await _cacheService.SetClinicalNoteAsync(note);
        await _cacheService.InvalidatePatientClinicalNotesAsync(note.PatientId);

        _logger.LogInformation("Clinical note {NoteId} created (Draft)", note.Id);

        return _mapper.MapClinicalNoteToResponse(note);
    }
}
