using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Queries;

/// <summary>
/// Get vital signs timeline query handler.
/// Returns vital signs for patient within optional date range.
/// Cached for historical data.
/// </summary>
public class GetVitalSignsTimelineQueryHandler : IQueryHandler<GetVitalSignsTimelineQuery, VitalSignsTimelineResponse>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetVitalSignsTimelineQueryHandler> _logger;

    public GetVitalSignsTimelineQueryHandler(
        IUnitOfWork unitOfWork,
        ILogger<GetVitalSignsTimelineQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<VitalSignsTimelineResponse> Handle(
        GetVitalSignsTimelineQuery query,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Getting vital signs timeline for patient {PatientId}, from {From} to {To}",
            query.PatientId, query.FromDate, query.ToDate);

        var repository = _unitOfWork.Repository<Domain.ClinicalNote>();
        
        // Get all clinical notes for patient
        var notes = await repository.GetAsync(
            q => q.Where(n => n.PatientId == query.PatientId),
            cancellationToken);

        // Extract vitals
        var vitals = notes
            .SelectMany(n => n.Vitals)
            .Where(v => 
                (!query.FromDate.HasValue || v.RecordedAt >= query.FromDate) &&
                (!query.ToDate.HasValue || v.RecordedAt <= query.ToDate))
            .OrderByDescending(v => v.RecordedAt)
            .ToList();

        // Group by vital type
        var groupedVitals = vitals
            .GroupBy(v => v.VitalType)
            .Select(g => new VitalTypeTimeline
            {
                VitalType = g.Key,
                Records = g.Select(v => new VitalRecord
                {
                    Value = v.Value,
                    Unit = v.Unit,
                    RecordedAt = v.RecordedAt
                }).ToList()
            })
            .ToList();

        return new VitalSignsTimelineResponse
        {
            PatientId = query.PatientId,
            FromDate = query.FromDate,
            ToDate = query.ToDate,
            TotalRecords = vitals.Count,
            VitalsByType = groupedVitals
        };
    }
}

/// <summary>
/// Vital signs timeline response
/// </summary>
public class VitalSignsTimelineResponse
{
    public Guid PatientId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int TotalRecords { get; set; }
    public List<VitalTypeTimeline> VitalsByType { get; set; } = new();
}

/// <summary>
/// Vital type with timeline of records
/// </summary>
public class VitalTypeTimeline
{
    public string VitalType { get; set; }
    public List<VitalRecord> Records { get; set; } = new();
}

/// <summary>
/// Individual vital record
/// </summary>
public class VitalRecord
{
    public string Value { get; set; }
    public string Unit { get; set; }
    public DateTime RecordedAt { get; set; }
}
