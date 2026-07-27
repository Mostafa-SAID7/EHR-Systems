using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.DTOs;
using EHRPlatform.Services.Clinical.Domain.Entities;
using EHRPlatform.Services.Clinical.Application.ClinicalNoteManagement.Responses;

namespace EHRPlatform.Services.Clinical.Features.ClinicalNotes.Queries;

/// <summary>
/// Get patient clinical timeline handler.
/// Single Responsibility: Retrieve paginated clinical notes for a patient's encounter timeline.
/// </summary>
public class GetPatientClinicalTimelineQueryHandler : IQueryHandler<GetPatientClinicalTimelineQuery, PagedResult<ClinicalNoteTimelineItemDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPatientClinicalTimelineQueryHandler> _logger;

    public GetPatientClinicalTimelineQueryHandler(IUnitOfWork unitOfWork, ILogger<GetPatientClinicalTimelineQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<PagedResult<ClinicalNoteTimelineItemDto>> Handle(
        GetPatientClinicalTimelineQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching clinical timeline for patient {PatientId}", request.PatientId);

        var repo = _unitOfWork.Repository<ClinicalNote>();
        var skip = (request.PageNumber - 1) * request.PageSize;

        var total = await repo.CountAsync(q => q.Where(n => n.PatientId == request.PatientId), cancellationToken);
        var notes = await repo.ToListAsync(
            q => q.Where(n => n.PatientId == request.PatientId)
                  .OrderByDescending(n => n.EncounterDate)
                  .Skip(skip)
                  .Take(request.PageSize),
            cancellationToken);

        var timelineItems = notes.Select(n => new ClinicalNoteTimelineItemDto
        {
            Id = n.Id,
            EncounterDate = n.EncounterDate,
            EncounterType = n.EncounterType,
            Status = n.Status,
            ProviderId = n.ProviderId,
            Diagnoses = n.Diagnoses.Select(d => new DiagnosisDto
            {
                Id = d.Id,
                DiagnosisCode = d.DiagnosisCode,
                DiagnosisText = d.DiagnosisText,
                DiagnosisType = d.DiagnosisType
            }).ToList(),
            LatestVitals = n.VitalSigns.OrderByDescending(v => v.RecordedAt).FirstOrDefault() is VitalSigns v
                ? new VitalSignsDto
                {
                    Id = v.Id,
                    RecordedAt = v.RecordedAt,
                    Temperature = v.Temperature,
                    SystolicBP = v.SystolicBP,
                    DiastolicBP = v.DiastolicBP,
                    HeartRate = v.HeartRate,
                    RespiratoryRate = v.RespiratoryRate,
                    Weight = v.Weight
                }
                : null
        }).ToList();

        return PagedResult<ClinicalNoteTimelineItemDto>.Create(timelineItems, total, request.PageNumber, request.PageSize);
    }
}
