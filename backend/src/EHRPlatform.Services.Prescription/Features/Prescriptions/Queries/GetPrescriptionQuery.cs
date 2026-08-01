using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Contracts.DTOs;
using EHRPlatform.Services.Prescription.Application.PrescriptionManagement.Responses;

namespace EHRPlatform.Services.Prescription.Features.Prescriptions.Queries;

/// <summary>
/// Get prescription by ID - CACHED query.
/// </summary>
public record GetPrescriptionQuery : ICachedQuery<PrescriptionResponseDto>
{
    public Guid PrescriptionId { get; init; }

    public string CacheKey => $"prescription_{PrescriptionId}";
    public int CacheDurationSeconds => 600; // 10 minutes
}

/// <summary>
/// Get patient active prescriptions - CACHED query.
/// </summary>
public record GetPatientActivePrescriptionsQuery : ICachedQuery<PagedResult<PrescriptionResponseDto>>
{
    public Guid PatientId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;

    public string CacheKey => $"prescriptions_patient_{PatientId}_active_{PageNumber}_{PageSize}";
    public int CacheDurationSeconds => 600;
}

/// <summary>
/// Get patient all prescriptions history - CACHED query.
/// </summary>
public record GetPatientPrescriptionHistoryQuery : ICachedQuery<PagedResult<PrescriptionResponseDto>>
{
    public Guid PatientId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;

    public string CacheKey => $"prescriptions_patient_{PatientId}_history_{PageNumber}_{PageSize}";
    public int CacheDurationSeconds => 600;
}

/// <summary>
/// Get pending refill requests for provider - CACHED query.
/// </summary>
public record GetPendingRefillsQuery : ICachedQuery<PagedResult<RefillRequestDto>>
{
    public Guid ProviderId { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 50;

    public string CacheKey => $"refills_provider_{ProviderId}_{PageNumber}_{PageSize}";
    public int CacheDurationSeconds => 300; // 5 minutes - more frequently updated
}


