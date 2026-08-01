using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EHRPlatform.BuildingBlocks.Common.Application.Common.Models;
using EHRPlatform.Services.Clinical.Application.Mappers;
using EHRPlatform.Services.Clinical.Contracts.Responses;
using EHRPlatform.Services.Clinical.Persistence;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Queries;

/// <summary>
/// Handles GetPatientClinicalTimelineQuery.
/// Returns paginated clinical notes for a patient, sorted by encounter date descending.
/// </summary>
public class GetPatientClinicalTimelineQueryHandler
    : IRequestHandler<GetPatientClinicalTimelineQuery, PagedResult<ClinicalNoteResponse>>
{
    private readonly ClinicalContext _context;
    private readonly ClinicalDtoMapper _mapper;
    private readonly ILogger<GetPatientClinicalTimelineQueryHandler> _logger;

    public GetPatientClinicalTimelineQueryHandler(
        ClinicalContext context,
        ClinicalDtoMapper mapper,
        ILogger<GetPatientClinicalTimelineQueryHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<ClinicalNoteResponse>> Handle(
        GetPatientClinicalTimelineQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogDebug(
            "Fetching clinical timeline for Patient {PatientId} — Page {Page}/{PageSize}",
            query.PatientId, query.PageNumber, query.PageSize);

        var baseQuery = _context.ClinicalNotes
            .AsNoTracking()
            .Where(n => n.PatientId == query.PatientId)
            .OrderByDescending(n => n.EncounterDate);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var notes = await baseQuery
            .Include(n => n.VitalSigns)
            .Include(n => n.Diagnoses)
            .Include(n => n.Procedures)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var items = notes.Select(_mapper.MapClinicalNoteToResponse).ToList();

        return new PagedResult<ClinicalNoteResponse>
        {
            Items      = items,
            TotalCount = totalCount,
            PageNumber = query.PageNumber,
            PageSize   = query.PageSize
        };
    }
}
