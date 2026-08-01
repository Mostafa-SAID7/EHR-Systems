using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Clinical.Application.Mappers;
using EHRPlatform.Services.Clinical.Contracts.Responses;
using EHRPlatform.Services.Clinical.Persistence;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Queries;

/// <summary>
/// Handles GetVitalSignsTimelineQuery.
/// Returns all vital signs readings for a patient with optional date range filter.
/// Results ordered by RecordedAt descending (most recent first).
/// </summary>
public class GetVitalSignsTimelineQueryHandler
    : IRequestHandler<GetVitalSignsTimelineQuery, List<VitalSignsResponse>>
{
    private readonly ClinicalContext _context;
    private readonly ClinicalDtoMapper _mapper;
    private readonly ILogger<GetVitalSignsTimelineQueryHandler> _logger;

    public GetVitalSignsTimelineQueryHandler(
        ClinicalContext context,
        ClinicalDtoMapper mapper,
        ILogger<GetVitalSignsTimelineQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<List<VitalSignsResponse>> Handle(
        GetVitalSignsTimelineQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Fetching vitals timeline for Patient {PatientId} — From {From} To {To}",
            query.PatientId, query.FromDate, query.ToDate);

        // Join VitalSigns through ClinicalNote.PatientId
        var vitalsQuery = _context.VitalSigns
            .AsNoTracking()
            .Include(v => v.ClinicalNote)
            .Where(v => v.ClinicalNote.PatientId == query.PatientId);

        if (query.FromDate.HasValue)
            vitalsQuery = vitalsQuery.Where(v => v.RecordedAt >= query.FromDate.Value);

        if (query.ToDate.HasValue)
            vitalsQuery = vitalsQuery.Where(v => v.RecordedAt <= query.ToDate.Value);

        var vitals = await vitalsQuery
            .OrderByDescending(v => v.RecordedAt)
            .ToListAsync(cancellationToken);

        return vitals.Select(_mapper.MapVitalSignsToResponse).ToList();
    }
}
