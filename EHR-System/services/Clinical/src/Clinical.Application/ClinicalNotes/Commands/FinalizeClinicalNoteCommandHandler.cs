using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Clinical.Application.Mappers;
using EHRPlatform.Services.Clinical.Application.Services;
using EHRPlatform.Services.Clinical.Contracts.Responses;
using EHRPlatform.Services.Clinical.Persistence;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Commands;

/// <summary>
/// Handles FinalizeClinicalNoteCommand.
/// Locks the note, preventing further edits, and publishes ClinicalNoteCompletedEvent.
/// </summary>
public class FinalizeClinicalNoteCommandHandler : IRequestHandler<FinalizeClinicalNoteCommand, ClinicalNoteResponse>
{
    private readonly ClinicalContext _context;
    private readonly ClinicalCacheService _cacheService;
    private readonly ClinicalDtoMapper _mapper;
    private readonly ILogger<FinalizeClinicalNoteCommandHandler> _logger;

    public FinalizeClinicalNoteCommandHandler(
        ClinicalContext context,
        ClinicalCacheService cacheService,
        ClinicalDtoMapper mapper,
        ILogger<FinalizeClinicalNoteCommandHandler> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ClinicalNoteResponse> Handle(
        FinalizeClinicalNoteCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finalizing clinical note {NoteId}", command.ClinicalNoteId);

        var note = await _context.ClinicalNotes
            .Include(n => n.VitalSigns)
            .Include(n => n.Diagnoses)
            .Include(n => n.Procedures)
            .FirstOrDefaultAsync(n => n.Id == command.ClinicalNoteId, cancellationToken)
            ?? throw new KeyNotFoundException($"Clinical note {command.ClinicalNoteId} not found");

        // Domain method validates status and raises ClinicalNoteCompletedEvent
        note.Finalize();
        note.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Invalidate all cache entries related to this note
        await _cacheService.InvalidateClinicalNoteAsync(note.Id);
        await _cacheService.InvalidatePatientClinicalNotesAsync(note.PatientId);

        _logger.LogInformation(
            "Clinical note {NoteId} finalized for Patient {PatientId}",
            note.Id, note.PatientId);

        return _mapper.MapClinicalNoteToResponse(note);
    }
}
