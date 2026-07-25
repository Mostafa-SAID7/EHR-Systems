using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Clinical.Domain.Entities;
using EHRPlatform.Services.Clinical.Application.ClinicalNoteManagement.Responses;
using Microsoft.EntityFrameworkCore;
using Mapster;

namespace EHRPlatform.Services.Clinical.Features.ClinicalNotes.Queries;

/// <summary>
/// Get clinical note by ID handler.
/// </summary>
public class GetClinicalNoteQueryHandler : IQueryHandler<GetClinicalNoteQuery, ClinicalNoteResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetClinicalNoteQueryHandler> _logger;

    public GetClinicalNoteQueryHandler(IUnitOfWork unitOfWork, ILogger<GetClinicalNoteQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ClinicalNoteResponseDto> Handle(
        GetClinicalNoteQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching clinical note {NoteId}", request.ClinicalNoteId);

        var repo = _unitOfWork.Repository<ClinicalNote>();
        var note = await repo.FirstOrDefaultAsync(
            q => q.Where(n => n.Id == request.ClinicalNoteId),
            cancellationToken);

        if (note == null)
            throw new InvalidOperationException($"Clinical note {request.ClinicalNoteId} not found");

        var dto = note.Adapt<ClinicalNoteResponseDto>();
        dto.VitalSigns = note.VitalSigns.Select(v => new VitalSignsDto
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

        dto.Diagnoses = note.Diagnoses.Select(d => new DiagnosisDto
        {
            Id = d.Id,
            DiagnosisCode = d.DiagnosisCode,
            DiagnosisText = d.DiagnosisText,
            DiagnosisType = d.DiagnosisType
        }).ToList();

        dto.Procedures = note.Procedures.Select(p => new ProcedureDto
        {
            Id = p.Id,
            ProcedureName = p.ProcedureName,
            ProcedureCode = p.ProcedureCode,
            PerformedDate = p.PerformedDate
        }).ToList();

        return dto;
    }
}

/// <summary>
/// Get patient clinical timeline handler.
/// </summary>
public class GetPatientClinicalTimelineQueryHandler : IQueryHandler<GetPatientClinicalTimelineQuery, ClinicalNoteListDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetPatientClinicalTimelineQueryHandler> _logger;

    public GetPatientClinicalTimelineQueryHandler(IUnitOfWork unitOfWork, ILogger<GetPatientClinicalTimelineQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ClinicalNoteListDto> Handle(
        GetPatientClinicalTimelineQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching clinical timeline for patient {PatientId}", request.PatientId);

        var repo = _unitOfWork.Repository<ClinicalNote>();
        var skip = (request.PageNumber - 1) * request.PageSize;

        var query = repo.Query()
            .Where(n => n.PatientId == request.PatientId)
            .OrderByDescending(n => n.EncounterDate);

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

        return new ClinicalNoteListDto
        {
            PatientId = request.PatientId,
            Notes = timelineItems,
            Total = total,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };
    }
}

/// <summary>
/// Get vital signs timeline handler.
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
        var notes = await noteRepo.Query()
            .Where(n => n.PatientId == request.PatientId)
            .ToListAsync(cancellationToken);

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

/// <summary>
/// Get diagnosis history handler.
/// </summary>
public class GetDiagnosisHistoryQueryHandler : IQueryHandler<GetDiagnosisHistoryQuery, DiagnosisDetailDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<GetDiagnosisHistoryQueryHandler> _logger;

    public GetDiagnosisHistoryQueryHandler(IUnitOfWork unitOfWork, ILogger<GetDiagnosisHistoryQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<DiagnosisDetailDto> Handle(
        GetDiagnosisHistoryQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching diagnosis history for patient {PatientId}", request.PatientId);

        var noteRepo = _unitOfWork.Repository<ClinicalNote>();
        var notes = await noteRepo.Query()
            .Where(n => n.PatientId == request.PatientId)
            .OrderByDescending(n => n.EncounterDate)
            .ToListAsync(cancellationToken);

        var diagnoses = notes
            .SelectMany(n => n.Diagnoses.Select(d => new DiagnosisHistoryItemDto
            {
                Id = d.Id,
                DiagnosisCode = d.DiagnosisCode,
                DiagnosisText = d.DiagnosisText,
                DiagnosisType = d.DiagnosisType,
                RecordedDate = n.EncounterDate,
                ClinicalNoteId = n.Id
            }))
            .OrderByDescending(d => d.RecordedDate)
            .ToList();

        return new DiagnosisDetailDto
        {
            PatientId = request.PatientId,
            Diagnoses = diagnoses
        };
    }
}
