using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Clinical.Application.Mappers;
using EHRPlatform.Services.Clinical.Application.Services;
using EHRPlatform.Services.Clinical.Contracts.Responses;
using EHRPlatform.Services.Clinical.Persistence;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Commands;

/// <summary>
/// Handles RecordVitalsCommand.
/// Records vital signs on a clinical note and publishes VitalSignsRecordedEvent.
/// </summary>
public class RecordVitalsCommandHandler : IRequestHandler<RecordVitalsCommand, ClinicalNoteResponse>
{
    private readonly ClinicalContext _context;
    private readonly ClinicalCacheService _cacheService;
    private readonly ClinicalDtoMapper _mapper;
    private readonly ILogger<RecordVitalsCommandHandler> _logger;

    public RecordVitalsCommandHandler(
        ClinicalContext context,
        ClinicalCacheService cacheService,
        ClinicalDtoMapper mapper,
        ILogger<RecordVitalsCommandHandler> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ClinicalNoteResponse> Handle(
        RecordVitalsCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recording vitals for clinical note {NoteId}", command.ClinicalNoteId);

        var note = await _context.ClinicalNotes
            .Include(n => n.VitalSigns)
            .Include(n => n.Diagnoses)
            .Include(n => n.Procedures)
            .FirstOrDefaultAsync(n => n.Id == command.ClinicalNoteId, cancellationToken)
            ?? throw new KeyNotFoundException($"Clinical note {command.ClinicalNoteId} not found");

        if (note.Status == "Finalized")
            throw new InvalidOperationException("Cannot add vitals to a finalized clinical note");

        // Domain method — also raises VitalSignsRecordedEvent
        note.RecordVitals(
            command.Temperature,
            command.SystolicBP,
            command.DiastolicBP,
            command.HeartRate,
            command.RespiratoryRate,
            command.Weight);

        await _context.SaveChangesAsync(cancellationToken);

        // Invalidate cache so queries read fresh data
        await _cacheService.InvalidateClinicalNoteAsync(note.Id);
        await _cacheService.InvalidateVitalSignsAsync(note.VitalSigns.Last().Id, note.PatientId);

        _logger.LogInformation("Vitals recorded for note {NoteId}, Patient {PatientId}", note.Id, note.PatientId);

        return _mapper.MapClinicalNoteToResponse(note);
    }
}
