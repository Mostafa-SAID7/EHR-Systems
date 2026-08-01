using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Contracts.DTOs;
using EHRPlatform.Services.Clinical.Application.ClinicalNoteManagement.Responses;
using EHRPlatform.Services.Clinical.Application.ClinicalNotes.Responses;

namespace EHRPlatform.Services.Clinical.Features.ClinicalNotes.Queries;

/// <summary>
/// Get clinical note by ID - CACHED query.
/// </summary>
public record GetClinicalNoteQuery : ICachedQuery<ClinicalNoteResponseDto>
{
    public Guid ClinicalNoteId { get; init; }

    public string CacheKey => $"clinical_note_{ClinicalNoteId}";
    public int CacheDurationSeconds => 900; // 15 minutes
}

/// <summary>
/// Get patient clinical timeline.
/// All clinical notes with vitals and diagnoses.
/// CACHED query.
/// </summary>
public record GetPatientClinicalTimelineQuery : ICachedQuery<PagedResult<ClinicalNoteTimelineItemDto>>
{
    public Guid PatientId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    public string CacheKey => $"clinical_timeline_{PatientId}_{PageNumber}_{PageSize}";
    public int CacheDurationSeconds => 600; // 10 minutes
}

/// <summary>
/// Get paginated list of clinical notes for a patient.
/// CACHED query.
/// </summary>
public record GetClinicalNotesQuery : ICachedQuery<PagedResult<ClinicalNoteResponse>>
{
    public Guid PatientId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Status { get; init; }

    public string CacheKey => $"clinical_notes_{PatientId}_{Status}_{PageNumber}_{PageSize}";
    public int CacheDurationSeconds => 300; // 5 minutes
}

/// <summary>
/// Get patient vital signs timeline.
/// Time-series vital records.
/// CACHED query.
/// </summary>
public record GetVitalSignsTimelineQuery : ICachedQuery<VitalSignsDetailDto>
{
    public Guid PatientId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }

    public string CacheKey => $"vitals_timeline_{PatientId}_{FromDate:yyyyMMdd}_{ToDate:yyyyMMdd}";
    public int CacheDurationSeconds => 600;
}

/// <summary>
/// Get patient diagnoses history.
/// All ICD-10 diagnoses with dates.
/// CACHED query.
/// </summary>
public record GetDiagnosisHistoryQuery : ICachedQuery<DiagnosisDetailDto>
{
    public Guid PatientId { get; init; }

    public string CacheKey => $"diagnosis_history_{PatientId}";
    public int CacheDurationSeconds => 900;
}


