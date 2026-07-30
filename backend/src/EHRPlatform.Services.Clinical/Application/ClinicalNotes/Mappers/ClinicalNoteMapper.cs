using EHRPlatform.Common.Application.Mapping;
using EHRPlatform.Common.Shared.DTOs;
using EHRPlatform.Services.Clinical.Domain.Entities;
using EHRPlatform.Services.Clinical.Application.ClinicalNotes.Responses;
using Microsoft.Extensions.Logging;
using VitalSignsDto = EHRPlatform.Services.Clinical.Application.ClinicalNotes.Responses.VitalSignsDto;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNotes.Mappers;

/// <summary>
/// Clinical Note Mapper
/// Single Responsibility: Convert between ClinicalNote domain models and DTOs.
/// </summary>
public class ClinicalNoteMapper : MappingServiceBase<Domain.Entities.ClinicalNote, ClinicalNoteResponse>
{
    public ClinicalNoteMapper(ILogger<ClinicalNoteMapper> logger) : base(logger)
    {
    }

    public ClinicalNoteResponse MapToResponse(Domain.Entities.ClinicalNote note)
    {
        Logger.LogDebug("Mapping clinical note {NoteId} to response DTO", note.Id);

        return new ClinicalNoteResponse
        {
            Id = note.Id,
            PatientId = note.PatientId,
            ProviderId = note.ProviderId,
            EncounterDate = note.EncounterDate,
            EncounterType = note.EncounterType,
            Status = note.Status,
            Subjective = note.Subjective,
            Objective = note.Objective,
            Assessment = note.Assessment,
            Plan = note.Plan,
            VitalSigns = note.VitalSigns
                .Select(v => new VitalSignsDto
                {
                    Id = v.Id,
                    Temperature = v.Temperature,
                    SystolicBP = v.SystolicBP,
                    DiastolicBP = v.DiastolicBP,
                    HeartRate = v.HeartRate,
                    RespiratoryRate = v.RespiratoryRate,
                    Weight = v.Weight,
                    RecordedAt = v.RecordedAt
                })
                .ToList(),
            Diagnoses = note.Diagnoses
                .Select(d => new ClinicalDiagnosisDto
                {
                    Id = d.Id,
                    DiagnosisCode = d.DiagnosisCode,
                    DiagnosisText = d.DiagnosisText,
                    DiagnosisType = d.DiagnosisType
                })
                .ToList(),
            Procedures = note.Procedures
                .Select(p => new ClinicalProcedureDto
                {
                    Id = p.Id,
                    ProcedureName = p.ProcedureName,
                    ProcedureCode = p.ProcedureCode,
                    Result = p.Result,
                    PerformedAt = p.PerformedAt
                })
                .ToList(),
            CreatedAt = note.CreatedAt,
            UpdatedAt = note.UpdatedAt
        };
    }

    public PagedResult<ClinicalNoteResponse> MapToPagedResult(
        ICollection<Domain.Entities.ClinicalNote> notes,
        int total,
        int pageNumber,
        int pageSize)
    {
        Logger.LogDebug("Mapping {Count} clinical notes to paged result", notes.Count);

        var dtos = notes.Select(n => MapToResponse(n)).ToList();
        return PagedResult<ClinicalNoteResponse>.Create(dtos, total, pageNumber, pageSize);
    }
}

