using Mapster;
using EHRPlatform.Services.Clinical.Domain.Entities;
using EHRPlatform.Services.Clinical.Application.ClinicalNoteManagement.Responses;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNoteManagement.Mappers;

/// <summary>
/// Mapster registration profile for Clinical entity mappings.
/// Handles conversion between domain models and DTOs.
/// Single Responsibility: Configure all Clinical-related type mappings.
/// </summary>
public class ClinicalNoteMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // ClinicalNote → ClinicalNoteResponseDto
        config.NewConfig<ClinicalNote, ClinicalNoteResponseDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.PatientId, src => src.PatientId)
            .Map(dest => dest.ProviderId, src => src.ProviderId)
            .Map(dest => dest.EncounterDate, src => src.EncounterDate)
            .Map(dest => dest.EncounterType, src => src.EncounterType)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.Subjective, src => src.Subjective)
            .Map(dest => dest.Objective, src => src.Objective)
            .Map(dest => dest.Assessment, src => src.Assessment)
            .Map(dest => dest.Plan, src => src.Plan)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.LastModifiedAt, src => src.LastModifiedAt);

        // ClinicalNote → ClinicalNoteTimelineItemDto
        config.NewConfig<ClinicalNote, ClinicalNoteTimelineItemDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.EncounterDate, src => src.EncounterDate)
            .Map(dest => dest.EncounterType, src => src.EncounterType)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.ProviderId, src => src.ProviderId)
            .Map(dest => dest.Diagnoses, src => src.Diagnoses.Adapt<List<DiagnosisDto>>())
            .Map(dest => dest.LatestVitals, src => src.VitalSigns.FirstOrDefault().Adapt<VitalSignsDto>());

        // VitalSigns → VitalSignsDto
        config.NewConfig<VitalSigns, VitalSignsDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.RecordedAt, src => src.RecordedAt)
            .Map(dest => dest.Temperature, src => src.Temperature)
            .Map(dest => dest.SystolicBP, src => src.SystolicBP)
            .Map(dest => dest.DiastolicBP, src => src.DiastolicBP)
            .Map(dest => dest.HeartRate, src => src.HeartRate)
            .Map(dest => dest.RespiratoryRate, src => src.RespiratoryRate)
            .Map(dest => dest.Weight, src => src.Weight);

        // ClinicalDiagnosis → DiagnosisDto
        config.NewConfig<ClinicalDiagnosis, DiagnosisDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.DiagnosisCode, src => src.DiagnosisCode)
            .Map(dest => dest.DiagnosisText, src => src.DiagnosisText)
            .Map(dest => dest.DiagnosisType, src => src.DiagnosisType);

        // ClinicalProcedure → ProcedureDto
        config.NewConfig<ClinicalProcedure, ProcedureDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ProcedureName, src => src.ProcedureName)
            .Map(dest => dest.ProcedureCode, src => src.ProcedureCode)
            .Map(dest => dest.PerformedDate, src => src.PerformedAt);

        // VitalSigns → VitalSignsRecordDto
        config.NewConfig<VitalSigns, VitalSignsRecordDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.RecordedAt, src => src.RecordedAt)
            .Map(dest => dest.Temperature, src => src.Temperature)
            .Map(dest => dest.SystolicBP, src => src.SystolicBP)
            .Map(dest => dest.DiastolicBP, src => src.DiastolicBP)
            .Map(dest => dest.HeartRate, src => src.HeartRate)
            .Map(dest => dest.RespiratoryRate, src => src.RespiratoryRate)
            .Map(dest => dest.Weight, src => src.Weight);

        // ClinicalDiagnosis → DiagnosisHistoryItemDto
        config.NewConfig<ClinicalDiagnosis, DiagnosisHistoryItemDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.DiagnosisCode, src => src.DiagnosisCode)
            .Map(dest => dest.DiagnosisText, src => src.DiagnosisText)
            .Map(dest => dest.DiagnosisType, src => src.DiagnosisType)
            .Map(dest => dest.RecordedDate, src => src.CreatedAt)
            .Map(dest => dest.ClinicalNoteId, src => src.ClinicalNoteId);

        // ClinicalProcedure → ProcedureDetailDto
        config.NewConfig<ClinicalProcedure, ProcedureDetailDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.ProcedureCode, src => src.ProcedureCode)
            .Map(dest => dest.ProcedureName, src => src.ProcedureName)
            .Map(dest => dest.PerformedDate, src => src.PerformedAt)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.ClinicalNoteId, src => src.ClinicalNoteId);

        // ClinicalNoteResponseDto → ClinicalNote (for updates)
        config.NewConfig<ClinicalNoteResponseDto, ClinicalNote>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.PatientId, src => src.PatientId)
            .Map(dest => dest.ProviderId, src => src.ProviderId)
            .Map(dest => dest.EncounterDate, src => src.EncounterDate)
            .Map(dest => dest.EncounterType, src => src.EncounterType)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.Subjective, src => src.Subjective)
            .Map(dest => dest.Objective, src => src.Objective)
            .Map(dest => dest.Assessment, src => src.Assessment)
            .Map(dest => dest.Plan, src => src.Plan);
    }
}
