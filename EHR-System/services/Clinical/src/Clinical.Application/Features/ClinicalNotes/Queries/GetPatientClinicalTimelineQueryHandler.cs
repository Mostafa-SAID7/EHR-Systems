using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.Services.Clinical.Contracts.Responses;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Queries;

/// <summary>
/// Get patient clinical timeline query handler.
/// Returns all clinical notes for patient (paginated).
/// Cached for performance.
/// </summary>
public class GetPatientClinicalTimelineQueryHandler : IQueryHandler<GetPatientClinicalTimelineQuery, PaginatedResponse<ClinicalNoteResponse>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPatientClinicalTimelineQueryHandler> _logger;

    public GetPatientClinicalTimelineQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetPatientClinicalTimelineQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PaginatedResponse<ClinicalNoteResponse>> Handle(
        GetPatientClinicalTimelineQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting clinical timeline for patient {PatientId}, page {PageNumber}",
            query.PatientId, query.PageNumber);

        var repository = _unitOfWork.Repository<Domain.ClinicalNote>();
        
        // Get total count
        var totalCount = await repository.CountAsync(
            q => q.Where(n => n.PatientId == query.PatientId),
            cancellationToken);

        // Get paginated results
        var notes = await repository.GetAsync(
            q => q
                .Where(n => n.PatientId == query.PatientId)
                .OrderByDescending(n => n.CreatedAt)
                .Skip((query.PageNumber - 1) * query.PageSize)
                .Take(query.PageSize),
            cancellationToken);

        var items = notes
            .Select(MapToResponse)
            .ToList();

        return new PaginatedResponse<ClinicalNoteResponse>
        {
            Items = items,
            PageNumber = query.PageNumber,
            PageSize = query.PageSize,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / query.PageSize)
        };
    }

    private static ClinicalNoteResponse MapToResponse(Domain.ClinicalNote note)
    {
        return new ClinicalNoteResponse
        {
            Id = note.Id,
            PatientId = note.PatientId,
            ProviderId = note.ProviderId,
            Status = note.Status,
            Subjective = note.Subjective,
            Objective = note.Objective,
            Assessment = note.Assessment,
            Plan = note.Plan,
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt
        };
    }
}

/// <summary>
/// Paginated response wrapper
/// </summary>
public class PaginatedResponse<T>
{
    public List<T> Items { get; set; } = new();
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
}
