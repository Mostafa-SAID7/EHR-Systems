using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.Services.Clinical.Domain.Entities;
using EHRPlatform.Services.Clinical.Application.ClinicalNoteManagement.Responses;
using Mapster;

namespace EHRPlatform.Services.Clinical.Features.ClinicalNotes.Queries;

/// <summary>
/// Get clinical note by ID handler.
/// Single Responsibility: Fetch and project a single clinical note including vitals, diagnoses, and procedures.
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
            PerformedDate = p.PerformedAt
        }).ToList();

        return dto;
    }
}


