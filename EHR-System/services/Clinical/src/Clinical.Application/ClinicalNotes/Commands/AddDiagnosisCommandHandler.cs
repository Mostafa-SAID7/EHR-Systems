using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Clinical.Application.Mappers;
using EHRPlatform.Services.Clinical.Application.Services;
using EHRPlatform.Services.Clinical.Contracts.Responses;
using EHRPlatform.Services.Clinical.Persistence;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Commands;

/// <summary>
/// Handles AddDiagnosisCommand.
/// Adds an ICD-10 diagnosis to a clinical note and publishes DiagnosisRecordedEvent.
/// </summary>
public class AddDiagnosisCommandHandler : IRequestHandler<AddDiagnosisCommand, ClinicalNoteResponse>
{
    private readonly ClinicalContext _context;
    private readonly ClinicalCacheService _cacheService;
    private readonly ClinicalDtoMapper _mapper;
    private readonly ILogger<AddDiagnosisCommandHandler> _logger;

    public AddDiagnosisCommandHandler(
        ClinicalContext context,
        ClinicalCacheService cacheService,
        ClinicalDtoMapper mapper,
        ILogger<AddDiagnosisCommandHandler> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ClinicalNoteResponse> Handle(
        AddDiagnosisCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Adding diagnosis {Code} to clinical note {NoteId}",
            command.DiagnosisCode, command.ClinicalNoteId);

        var note = await _context.ClinicalNotes
            .Include(n => n.VitalSigns)
            .Include(n => n.Diagnoses)
            .Include(n => n.Procedures)
            .FirstOrDefaultAsync(n => n.Id == command.ClinicalNoteId, cancellationToken)
            ?? throw new KeyNotFoundException($"Clinical note {command.ClinicalNoteId} not found");

        if (note.Status == "Finalized")
            throw new InvalidOperationException("Cannot add diagnosis to a finalized clinical note");

        // Domain method — also raises DiagnosisRecordedEvent
        note.AddDiagnosis(command.DiagnosisCode, command.DiagnosisText, command.DiagnosisType);

        await _context.SaveChangesAsync(cancellationToken);

        await _cacheService.InvalidateClinicalNoteAsync(note.Id);
        await _cacheService.InvalidateClinicalNoteDiagnosesAsync(note.Id);

        _logger.LogInformation(
            "Diagnosis {Code} added to note {NoteId}, Patient {PatientId}",
            command.DiagnosisCode, note.Id, note.PatientId);

        return _mapper.MapClinicalNoteToResponse(note);
    }
}
