using Mapster;
using EHRPlatform.BuildingBlocks.Common.Application.Mapping;
using EHRPlatform.BuildingBlocks.Contracts.DTOs;
using EHRPlatform.Services.Clinical.Domain.Entities;
using EHRPlatform.Services.Clinical.Application.ClinicalNoteManagement.Responses;

namespace EHRPlatform.Services.Clinical.Application.ClinicalNoteManagement.Mappers;

/// <summary>
/// Clinical Note Mapper
/// Single Responsibility: Convert between Clinical domain models and DTOs.
/// Handles all Clinical-related mappings with optional post-processing.
/// </summary>
public class ClinicalNoteMapper : MappingServiceBase<ClinicalNote, ClinicalNoteResponseDto>
{
    public ClinicalNoteMapper(ILogger<ClinicalNoteMapper> logger) : base(logger)
    {
    }

    /// <summary>
    /// Map single clinical note to response DTO.
    /// </summary>
    public ClinicalNoteResponseDto MapToResponseDto(ClinicalNote clinicalNote)
    {
        return MapSingleToDto(clinicalNote);
    }

    /// <summary>
    /// Map clinical note to detailed DTO with vitals, diagnoses, and procedures.
    /// </summary>
    public ClinicalNoteDetailedDto MapToDetailedDto(ClinicalNote clinicalNote)
    {
        Logger.LogDebug("Mapping clinical note {ClinicalNoteId} to detailed DTO", clinicalNote.Id);

        return new ClinicalNoteDetailedDto
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
            VitalSigns = clinicalNote.VitalSigns.Adapt<VitalSignsDto>(),
            Diagnoses = clinicalNote.Diagnoses.Adapt<List<DiagnosisDto>>(),
            Procedures = clinicalNote.Procedures.Adapt<List<ProcedureDto>>(),
            CreatedAt = clinicalNote.CreatedAt,
            LastModifiedAt = clinicalNote.UpdatedAt
        };
    }

    /// <summary>
    /// Map collection of clinical notes to paged result.
    /// </summary>
    public PagedResult<ClinicalNoteResponseDto> MapToPagedResult(
        ICollection<ClinicalNote> clinicalNotes,
        int total,
        int pageNumber,
        int pageSize)
    {
        Logger.LogDebug("Mapping {Count} clinical notes to paged result", clinicalNotes.Count);

        return PagedResult<ClinicalNoteResponseDto>.Create(
            clinicalNotes.Adapt<List<ClinicalNoteResponseDto>>(),
            total,
            pageNumber,
            pageSize);
    }

    /// <summary>
    /// Map collection of clinical notes to response DTO list.
    /// </summary>
    public List<ClinicalNoteResponseDto> MapToResponseDtoList(ICollection<ClinicalNote> clinicalNotes)
    {
        Logger.LogDebug("Mapping {Count} clinical notes to response DTO list", clinicalNotes.Count);
        return clinicalNotes.Adapt<List<ClinicalNoteResponseDto>>();
    }
}


