using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Clinical.Application.Mappers;
using EHRPlatform.Services.Clinical.Application.Services;
using EHRPlatform.Services.Clinical.Contracts.Responses;
using EHRPlatform.Services.Clinical.Persistence;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Queries;

/// <summary>
/// Handles GetClinicalNoteQuery.
/// Returns a single clinical note with all related entities (vitals, diagnoses, procedures).
/// Cache-first: Redis → PostgreSQL fallback.
/// </summary>
public class GetClinicalNoteQueryHandler : IRequestHandler<GetClinicalNoteQuery, ClinicalNoteResponse>
{
    private readonly ClinicalContext _context;
    private readonly ClinicalCacheService _cacheService;
    private readonly ClinicalDtoMapper _mapper;
    private readonly ILogger<GetClinicalNoteQueryHandler> _logger;

    public GetClinicalNoteQueryHandler(
        ClinicalContext context,
        ClinicalCacheService cacheService,
        ClinicalDtoMapper mapper,
        ILogger<GetClinicalNoteQueryHandler> logger)
    {
        _context = context;
        _cacheService = cacheService;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ClinicalNoteResponse> Handle(
        GetClinicalNoteQuery query,
        CancellationToken cancellationToken)
    {
        // Cache-first lookup
        var cached = await _cacheService.GetClinicalNoteAsync(query.ClinicalNoteId);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit for clinical note {NoteId}", query.ClinicalNoteId);
            return _mapper.MapClinicalNoteToResponse(cached);
        }

        _logger.LogDebug("Cache miss — querying PostgreSQL for note {NoteId}", query.ClinicalNoteId);

        var note = await _context.ClinicalNotes
            .AsNoTracking()
            .Include(n => n.VitalSigns)
            .Include(n => n.Diagnoses)
            .Include(n => n.Procedures)
            .FirstOrDefaultAsync(n => n.Id == query.ClinicalNoteId, cancellationToken)
            ?? throw new KeyNotFoundException($"Clinical note {query.ClinicalNoteId} not found");

        // Warm cache for future reads
        await _cacheService.SetClinicalNoteAsync(note);

        return _mapper.MapClinicalNoteToResponse(note);
    }
}
