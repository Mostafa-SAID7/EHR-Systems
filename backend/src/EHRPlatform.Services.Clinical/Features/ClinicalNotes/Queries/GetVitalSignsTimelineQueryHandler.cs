using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Clinical.Domain.Entities;
using EHRPlatform.Services.Clinical.Application.ClinicalNoteManagement.Responses;

namespace EHRPlatform.Services.Clinical.Features.ClinicalNotes.Queries;

/// <summary>
/// Get vital signs timeline handler.
/// Single Responsibility: Retrieve and compute vital sign statistics for a patient over a date range.
/// </summary>
public class GetVitalSignsTimelineQueryHandler : IQueryHandler<GetVitalSignsTimelineQuery, VitalSignsDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetVitalSignsTimelineQueryHandler> _logger;

    public GetVitalSignsTimelineQueryHandler(IUnitOfWork unitOfWork, ILogger<GetVitalSignsTimelineQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<VitalSignsDetailDto> Handle(
        GetVitalSignsTimelineQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching vitals timeline for patient {PatientId}", request.PatientId);

        var noteRepo = _unitOfWork.Repository<ClinicalNote>();
        var notes = await noteRepo.ToListAsync(
            q => q.Where(n => n.PatientId == request.PatientId),
            cancellationToken);

        var allVitals = notes
            .SelectMany(n => n.VitalSigns)
            .Where(v => v.RecordedAt >= (request.FromDate ?? DateTime.MinValue) &&
                        v.RecordedAt <= (request.ToDate ?? DateTime.MaxValue))
            .OrderBy(v => v.RecordedAt)
            .ToList();

        var records = allVitals.Select(v => new VitalSignsRecordDto
        {
            Id = v.Id,
            RecordedAt = v.RecordedAt,
            Temperature = v.Temperature,
            SystolicBP = v.SystolicBP,
            DiastolicBP = v.DiastolicBP,
            HeartRate = v.HeartRate,
            RespiratoryRate = v.RespiratoryRate,
            Weight = v.Weight
        }).ToList();

        var stats = new VitalSignsStatisticsDto
        {
            AverageTemperature = allVitals.Any() ? (decimal)allVitals.Average(v => (double)v.Temperature) : 0,
            AverageSystolicBP = allVitals.Any() ? (int)allVitals.Average(v => v.SystolicBP) : 0,
            AverageDiastolicBP = allVitals.Any() ? (int)allVitals.Average(v => v.DiastolicBP) : 0,
            AverageHeartRate = allVitals.Any() ? (int)allVitals.Average(v => v.HeartRate) : 0
        };

        return new VitalSignsDetailDto
        {
            PatientId = request.PatientId,
            Records = records,
            Statistics = stats
        };
    }
}

