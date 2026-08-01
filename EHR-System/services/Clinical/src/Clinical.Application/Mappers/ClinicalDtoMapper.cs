using EHRPlatform.BuildingBlocks.Common.Application.Mappers;
using EHRPlatform.Services.Clinical.Domain.Entities;
using EHRPlatform.Services.Clinical.Contracts.Responses;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Clinical.Application.Mappers;

/// <summary>
/// Mapper for Clinical DTOs.
/// Maps domain entities to response DTOs.
/// </summary>
public class ClinicalDtoMapper : BaseMapper
{
    public ClinicalDtoMapper(ILogger<ClinicalDtoMapper> logger) : base(logger) { }

    public ClinicalNoteResponse MapClinicalNoteToResponse(ClinicalNote clinicalNote)
    {
        return new ClinicalNoteResponse
        {
            Id = clinicalNote.Id,
            PatientId = clinicalNote.PatientId,
            ProviderId = clinicalNote.ProviderId,
            EncounterDate = clinicalNote.EncounterDate,
            EncounterType = clinicalNote.EncounterType,
            Status = clinicalNote.Status,
            Subjective = clinicalNote.Subjective,
            Objective = clinicalNote.Objective,
            Assessment = clinicalNote.Assessment,
            Plan = clinicalNote.Plan,
            VitalSigns = clinicalNote.VitalSigns.Select(MapVitalSignsToResponse).ToList(),
            Diagnoses = clinicalNote.Diagnoses.Select(MapDiagnosisToResponse).ToList(),
            Procedures = clinicalNote.Procedures.Select(MapProcedureToResponse).ToList(),
            CreatedAt = clinicalNote.CreatedAt,
            UpdatedAt = clinicalNote.UpdatedAt,
            CreatedBy = clinicalNote.CreatedBy,
            UpdatedBy = clinicalNote.UpdatedBy
        };
    }

    public VitalSignsResponse MapVitalSignsToResponse(VitalSigns vitalSigns)
    {
        return new VitalSignsResponse
        {
            Id = vitalSigns.Id,
            ClinicalNoteId = vitalSigns.ClinicalNoteId,
            RecordedAt = vitalSigns.RecordedAt,
            Temperature = vitalSigns.Temperature,
            SystolicBP = vitalSigns.SystolicBP,
            DiastolicBP = vitalSigns.DiastolicBP,
            HeartRate = vitalSigns.HeartRate,
            RespiratoryRate = vitalSigns.RespiratoryRate,
            Weight = vitalSigns.Weight
        };
    }

    public DiagnosisResponse MapDiagnosisToResponse(ClinicalDiagnosis diagnosis)
    {
        return new DiagnosisResponse
        {
            Id = diagnosis.Id,
            ClinicalNoteId = diagnosis.ClinicalNoteId,
            DiagnosisCode = diagnosis.DiagnosisCode,
            DiagnosisText = diagnosis.DiagnosisText,
            DiagnosisType = diagnosis.DiagnosisType
        };
    }

    public ProcedureResponse MapProcedureToResponse(ClinicalProcedure procedure)
    {
        return new ProcedureResponse
        {
            Id = procedure.Id,
            ClinicalNoteId = procedure.ClinicalNoteId,
            ProcedureName = procedure.ProcedureName,
            ProcedureCode = procedure.ProcedureCode,
            PerformedAt = procedure.PerformedAt,
            Result = procedure.Result
        };
    }

    public List<ClinicalNoteResponse> MapClinicalNotesToResponses(ICollection<ClinicalNote> clinicalNotes)
    {
        return clinicalNotes.Select(MapClinicalNoteToResponse).ToList();
    }
}
