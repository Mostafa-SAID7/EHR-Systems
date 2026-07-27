using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.DTOs;
using EHRPlatform.Services.Clinical.Features.ClinicalNotes.Queries;
using EHRPlatform.Services.Clinical.Application.ClinicalNotes.Responses;
using EHRPlatform.Services.Clinical.Application.ClinicalNotes.Mappers;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Clinical.Features.ClinicalNotes.Handlers;

/// <summary>
/// Get clinical notes query handler.
/// Retrieves paginated list of clinical notes for patient.
/// </summary>
public class GetClinicalNotesQueryHandler : IQueryHandler<GetClinicalNotesQuery, PagedResult<ClinicalNoteResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ClinicalNoteMapper _mapper;
    private readonly ILogger<GetClinicalNotesQueryHandler> _logger;

    public GetClinicalNotesQueryHandler(
        IUnitOfWork unitOfWork,
        ClinicalNoteMapper mapper,
        ILogger<GetClinicalNotesQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger;
    }

    public async Task<PagedResult<ClinicalNoteResponse>> Handle(GetClinicalNotesQuery query, CancellationToken cancellationToken)
    {
        _logger.LogDebug("Retrieving clinical notes for patient {PatientId}, page {PageNumber}", query.PatientId, query.PageNumber);

        var repo = _unitOfWork.Repository<Domain.Entities.ClinicalNote>();

        var skip = (query.PageNumber - 1) * query.PageSize;
        var total = await repo.CountAsync(
            q => q.Where(n => n.PatientId == query.PatientId && (query.Status == null || n.Status == query.Status)),
            cancellationToken);
        var notes = await repo.ToListAsync(
            q => q.Where(n => n.PatientId == query.PatientId && (query.Status == null || n.Status == query.Status))
                  .OrderByDescending(x => x.EncounterDate)
                  .Skip(skip)
                  .Take(query.PageSize),
            cancellationToken);

        return _mapper.MapToPagedResult(notes, total, query.PageNumber, query.PageSize);
    }
}
