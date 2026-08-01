namespace EHRPlatform.Services.Patient.Application.Services;

using EHRPlatform.Services.Patient.Application.Features.Patients.Queries;

/// <summary>
/// Service for caching patient data in Redis.
/// TTL: 10 minutes for single patient queries
/// </summary>
public interface IPatientCacheService
{
    /// <summary>
    /// Get patient from cache.
    /// </summary>
    Task<PatientDto?> GetPatientAsync(Guid patientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set patient in cache with specified TTL.
    /// </summary>
    Task SetPatientAsync(Guid patientId, PatientDto patient, TimeSpan? ttl = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidate patient cache.
    /// </summary>
    Task InvalidatePatientAsync(Guid patientId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get cached patient list.
    /// </summary>
    Task<List<PatientDto>?> GetPatientListAsync(int pageNumber, int pageSize, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set cached patient list.
    /// </summary>
    Task SetPatientListAsync(int pageNumber, int pageSize, List<PatientDto> patients, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidate all patient list caches.
    /// </summary>
    Task InvalidatePatientListAsync(CancellationToken cancellationToken = default);
}
