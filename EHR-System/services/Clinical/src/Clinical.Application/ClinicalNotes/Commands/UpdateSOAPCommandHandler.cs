using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Clinical.Application.Mappers;
using EHRPlatform.Services.Clinical.Application.Services;
using EHRPlatform.Services.Clinical.Contracts.Responses;
using EHRPlatform.Services.Clinical.Persistence;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Commands;

/// <summary>
/// Handles UpdateSOAPCommand.
/// Updates Subjective, Objective, Assessment, Plan fields on a draft clinical note.
/// </summary>
public class UpdateSOAPCommandHandler : IRequestHandler<UpdateSOAPCommand>
{
    private readonly ClinicalContext _context;
    private readonly ClinicalCacheService _cacheService;
    private readonly ILogger<UpdateSOAPCommandHandler> _logger;

    public UpdateSOAPCommandHandler(
        ClinicalContext context,
        ClinicalCacheService cacheService,
        ILogger<UpdateSOAPCommandHandler> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task Handle(UpdateSOAPCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating SOAP for clinical note {NoteId}", command.ClinicalNoteId);

        var note = await _context.ClinicalNotes
            .FirstOrDefaultAsync(n => n.Id == command.ClinicalNoteId, cancellationToken)
            ?? throw new KeyNotFoundException($"Clinical note {command.ClinicalNoteId} not found");

        if (note.Status == "Finalized")
            throw new InvalidOperationException("Cannot update a finalized clinical note");

        if (command.Subjective is not null) note.Subjective = command.Subjective;
        if (command.Objective  is not null) note.Objective  = command.Objective;
        if (command.Assessment is not null) note.Assessment = command.Assessment;
        if (command.Plan       is not null) note.Plan       = command.Plan;

        note.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        await _cacheService.InvalidateClinicalNoteAsync(note.Id);

        _logger.LogInformation("SOAP updated for clinical note {NoteId}", note.Id);
    }
}
