using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.Services.Patient.Application.PatientManagement.Responses;

namespace EHRPlatform.Services.Patient.Features.Patients.Queries;

/// <summary>
/// Get patient by MRN (Medical Record Number) - CACHED query.
/// MRN is unique and provides URL-friendly slug-based access.
/// Cache key uses slug to ensure consistency: "patient_mrn_{slug}"
/// </summary>
public record GetPatientByMRNQuery : ICachedQuery<PatientResponseDto>
{
    /// <summary>
    /// Medical Record Number (e.g., "MRN-2024-001234")
    /// </summary>
    public string MRN { get; init; } = string.Empty;

    public string CacheKey => $"patient_mrn_{MRN.ToLower().Replace(" ", "_")}";
    public int CacheDurationSeconds => 900; // 15 minutes
}

/// <summary>
/// Get patient detail by MRN (Medical Record Number) - CACHED query.
/// Includes allergies, conditions, and related data.
/// </summary>
public record GetPatientDetailByMRNQuery : ICachedQuery<PatientDetailDto>
{
    /// <summary>
    /// Medical Record Number (e.g., "MRN-2024-001234")
    /// </summary>
    public string MRN { get; init; } = string.Empty;

    public string CacheKey => $"patient_detail_mrn_{MRN.ToLower().Replace(" ", "_")}";
    public int CacheDurationSeconds => 900; // 15 minutes
}


