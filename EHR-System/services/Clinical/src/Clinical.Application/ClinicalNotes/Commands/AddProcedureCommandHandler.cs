using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Clinical.Application.Services;
using EHRPlatform.Services.Clinical.Persistence;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Commands;

/// <summary>
/// Handles AddProcedureCommand.
/// Adds a CPT/SNOMED procedure to a clinical note and publishes ProcedurePerformedEvent.
/// </summary>
public class AddProcedureCommandHandler : IRequestHandler<AddProcedureCommand>
{
    private readonly ClinicalContext _context;
    private readonly ClinicalCacheService _cacheService;
    private readonly ILogger<AddProcedureCommandHandler> _logger;

    public AddProcedureCommandHandler(
        ClinicalContext context,
        ClinicalCacheService cacheService,
        ILogger<AddProcedureCommandHandler> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task Handle(
        AddProcedureCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Adding procedure {Code} to clinical note {NoteId}",
            command.ProcedureCode, command.ClinicalNoteId);

        var note = await _context.ClinicalNotes
            .Include(n => n.VitalSigns)
            .Include(n => n.Diagnoses)
            .Include(n => n.Procedures)
            .FirstOrDefaultAsync(n => n.Id == command.ClinicalNoteId, cancellationToken)
            ?? throw new KeyNotFoundException($"Clinical note {command.ClinicalNoteId} not found");

        if (note.Status == "Finalized")
            throw new InvalidOperationException("Cannot add procedure to a finalized clinical note");

        // Domain method — also raises ProcedurePerformedEvent
        note.AddProcedure(command.ProcedureName, command.ProcedureCode, command.Result ?? string.Empty);

        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.InvalidateClinicalNoteAsync(note.Id);
        await _cacheService.InvalidateClinicalNoteProceduresAsync(note.Id);

        _logger.LogInformation(
            "Procedure {Code} added to note {NoteId}, Patient {PatientId}",
            command.ProcedureCode, note.Id, note.PatientId);
    }
}
