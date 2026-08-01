using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.Services.Patient.Application.PatientManagement.Responses;

namespace EHRPlatform.Services.Patient.Features.Patients.Queries;

/// <summary>
/// Get patient by ID - CACHED query.
/// Automatically cached for 15 minutes.
/// </summary>
public record GetPatientQuery : ICachedQuery<PatientResponseDto>
{
    public Guid PatientId { get; init; }

    public string CacheKey => $"patient_{PatientId}";
    public int CacheDurationSeconds => 900; // 15 minutes
}

/// <summary>
/// Search patients with pagination.
/// Full-text search with Elasticsearch.
/// </summary>
public record SearchPatientsQuery : ICachedQuery<SearchResultDto<PatientResponseDto>>
{
    public string SearchTerm { get; init; } = string.Empty;
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    public string CacheKey => $"patients_search_{SearchTerm}_{PageNumber}_{PageSize}";
    public int CacheDurationSeconds => 600; // 10 minutes
}

/// <summary>
/// List all patients with pagination.
/// Cached query.
/// </summary>
public record ListPatientsQuery : ICachedQuery<SearchResultDto<PatientResponseDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;

    public string CacheKey => $"patients_list_{PageNumber}_{PageSize}";
    public int CacheDurationSeconds => 600;
}

/// <summary>
/// Get patient with full details including allergies and conditions.
/// </summary>
public record GetPatientDetailQuery : ICachedQuery<PatientDetailDto>
{
    public Guid PatientId { get; init; }

    public string CacheKey => $"patient_detail_{PatientId}";
    public int CacheDurationSeconds => 900;
}


