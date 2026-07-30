using Microsoft.Extensions.Logging;

namespace EHRPlatform.Common.Infrastructure.Caching;

/// <summary>
/// Event-driven cache invalidation handler.
/// Automatically clears caches when domain events occur.
/// Typically triggered by Kafka consumers in microservices.
/// </summary>
public class CacheInvalidationEventHandler
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheInvalidationEventHandler> _logger;

    public CacheInvalidationEventHandler(ICacheService cacheService, ILogger<CacheInvalidationEventHandler> logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Handle patient-related events.
    /// </summary>
    public async Task HandlePatientEventAsync(string eventType, Guid patientId, CancellationToken cancellationToken = default)
    {
        try
        {
            switch (eventType)
            {
                case "PatientCreated":
                case "PatientUpdated":
                case "PatientDeleted":
                    // Invalidate patient-specific caches
                    await _cacheService.RemoveByPatternAsync(
                        $"patient:{patientId}:*",
                        cancellationToken);

                    // Invalidate patient list caches
                    await _cacheService.RemoveAsync(CacheKeyGenerator.PatientsListKey, cancellationToken);
                    await _cacheService.RemoveByPatternAsync("patients:paged:*", cancellationToken);
                    await _cacheService.RemoveByPatternAsync("patients:search:*", cancellationToken);

                    _logger.LogInformation("Invalidated caches for patient {PatientId} - Event: {EventType}",
                        patientId, eventType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error handling patient event invalidation: {EventType} for {PatientId}",
                eventType, patientId);
        }
    }

    /// <summary>
    /// Handle appointment-related events.
    /// </summary>
    public async Task HandleAppointmentEventAsync(string eventType, Guid appointmentId, Guid? patientId = null, 
        Guid? providerId = null, CancellationToken cancellationToken = default)
    {
        try
        {
            switch (eventType)
            {
                case "AppointmentCreated":
                case "AppointmentUpdated":
                case "AppointmentCancelled":
                    // Invalidate appointment-specific cache
                    await _cacheService.RemoveAsync(
                        CacheKeyGenerator.AppointmentKey(appointmentId),
                        cancellationToken);

                    // Invalidate patient's appointments if available
                    if (patientId.HasValue && patientId != Guid.Empty)
                    {
                        await _cacheService.RemoveAsync(
                            CacheKeyGenerator.AppointmentsByPatientKey(patientId.Value),
                            cancellationToken);
                    }

                    // Invalidate provider's appointments if available
                    if (providerId.HasValue && providerId != Guid.Empty)
                    {
                        await _cacheService.RemoveByPatternAsync(
                            $"appointments:provider:{providerId}:*",
                            cancellationToken);
                    }

                    _logger.LogInformation("Invalidated caches for appointment {AppointmentId} - Event: {EventType}",
                        appointmentId, eventType);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error handling appointment event invalidation: {EventType} for {AppointmentId}",
                eventType, appointmentId);
        }
    }

    /// <summary>
    /// Handle clinical-related events (SOAP notes, vitals, diagnoses).
    /// </summary>
    public async Task HandleClinicalEventAsync(string eventType, Guid clinicalDataId, Guid patientId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            switch (eventType)
            {
                case "SoapNoteCreated":
                case "SoapNoteUpdated":
                case "SoapNoteDeleted":
                    // Invalidate SOAP note caches
                    await _cacheService.RemoveAsync(
                        CacheKeyGenerator.SoapNoteKey(clinicalDataId),
                        cancellationToken);

                    // Invalidate patient's SOAP notes
                    await _cacheService.RemoveAsync(
                        CacheKeyGenerator.PatientSoapNotesKey(patientId),
                        cancellationToken);
                    break;

                case "VitalsUpdated":
                    // Invalidate patient vitals cache
                    await _cacheService.RemoveAsync(
                        CacheKeyGenerator.PatientVitalsKey(patientId),
                        cancellationToken);
                    break;

                case "DiagnosisAdded":
                case "DiagnosisUpdated":
                    // Invalidate patient diagnoses cache
                    await _cacheService.RemoveAsync(
                        CacheKeyGenerator.PatientDiagnosesKey(patientId),
                        cancellationToken);
                    break;

                case "AllergyAdded":
                case "AllergyUpdated":
                    // Invalidate patient allergies cache
                    await _cacheService.RemoveAsync(
                        CacheKeyGenerator.PatientAllergiesKey(patientId),
                        cancellationToken);
                    break;
            }

            _logger.LogInformation("Invalidated clinical caches for {PatientId} - Event: {EventType}",
                patientId, eventType);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error handling clinical event invalidation: {EventType}",
                eventType);
        }
    }

    /// <summary>
    /// Handle user-related events (authentication, permissions).
    /// </summary>
    public async Task HandleUserEventAsync(string eventType, Guid userId, CancellationToken cancellationToken = default)
    {
        try
        {
            switch (eventType)
            {
                case "UserCreated":
                case "UserUpdated":
                case "UserDeleted":
                    // Invalidate user-specific caches
                    await _cacheService.RemoveAsync(CacheKeyGenerator.UserKey(userId), cancellationToken);
                    await _cacheService.RemoveAsync(CacheKeyGenerator.UserRolesKey(userId), cancellationToken);
                    await _cacheService.RemoveAsync(CacheKeyGenerator.UserPermissionsKey(userId), cancellationToken);
                    break;

                case "RoleAssigned":
                case "RoleRevoked":
                    // Invalidate user roles
                    await _cacheService.RemoveAsync(CacheKeyGenerator.UserRolesKey(userId), cancellationToken);
                    await _cacheService.RemoveAsync(CacheKeyGenerator.UserPermissionsKey(userId), cancellationToken);
                    break;

                case "PermissionChanged":
                    // Invalidate user permissions
                    await _cacheService.RemoveAsync(CacheKeyGenerator.UserPermissionsKey(userId), cancellationToken);
                    break;
            }

            _logger.LogInformation("Invalidated user caches for {UserId} - Event: {EventType}",
                userId, eventType);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error handling user event invalidation: {EventType} for {UserId}",
                eventType, userId);
        }
    }

    /// <summary>
    /// Handle reference data events (code tables, lookups).
    /// </summary>
    public async Task HandleReferenceDataEventAsync(string dataType, CancellationToken cancellationToken = default)
    {
        try
        {
            // Invalidate reference data caches
            await _cacheService.RemoveAsync(
                CacheKeyGenerator.ReferenceDataKey(dataType),
                cancellationToken);

            _logger.LogInformation("Invalidated reference data cache for: {DataType}", dataType);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error handling reference data event invalidation: {DataType}", dataType);
        }
    }

    /// <summary>
    /// Generic invalidation for custom events.
    /// </summary>
    public async Task InvalidateCacheByPatternAsync(string pattern, CancellationToken cancellationToken = default)
    {
        try
        {
            await _cacheService.RemoveByPatternAsync(pattern, cancellationToken);
            _logger.LogInformation("Invalidated cache pattern: {Pattern}", pattern);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error invalidating cache pattern: {Pattern}", pattern);
        }
    }
}

/// <summary>
/// Fluent API helper for cache invalidation in command handlers.
/// </summary>
public static class CacheInvalidationExtensions
{
    /// <summary>
    /// Invalidate patient-related caches after patient command execution.
    /// </summary>
    public static async Task InvalidatePatientCacheAsync(
        this ICacheService cacheService,
        Guid patientId,
        bool includeList = true,
        CancellationToken cancellationToken = default)
    {
        await cacheService.RemoveByPatternAsync($"patient:{patientId}:*", cancellationToken);

        if (includeList)
        {
            await cacheService.RemoveAsync(CacheKeyGenerator.PatientsListKey, cancellationToken);
            await cacheService.RemoveByPatternAsync("patients:paged:*", cancellationToken);
            await cacheService.RemoveByPatternAsync("patients:search:*", cancellationToken);
        }
    }

    /// <summary>
    /// Invalidate appointment-related caches.
    /// </summary>
    public static async Task InvalidateAppointmentCacheAsync(
        this ICacheService cacheService,
        Guid appointmentId,
        Guid? patientId = null,
        CancellationToken cancellationToken = default)
    {
        await cacheService.RemoveAsync(
            CacheKeyGenerator.AppointmentKey(appointmentId),
            cancellationToken);

        if (patientId.HasValue && patientId != Guid.Empty)
        {
            await cacheService.RemoveAsync(
                CacheKeyGenerator.AppointmentsByPatientKey(patientId.Value),
                cancellationToken);
        }
    }

    /// <summary>
    /// Invalidate user-related caches.
    /// </summary>
    public static async Task InvalidateUserCacheAsync(
        this ICacheService cacheService,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        await cacheService.RemoveAsync(CacheKeyGenerator.UserKey(userId), cancellationToken);
        await cacheService.RemoveAsync(CacheKeyGenerator.UserRolesKey(userId), cancellationToken);
        await cacheService.RemoveAsync(CacheKeyGenerator.UserPermissionsKey(userId), cancellationToken);
    }
}

