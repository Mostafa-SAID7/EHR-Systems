using EHRPlatform.Common.Infrastructure.Caching;
using EHRPlatform.Services.Prescription.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Prescription.Application.Services;

/// <summary>
/// Distributed cache wrapper for Prescription Service.
/// Caches prescriptions and refill requests.
/// Uses ICacheService from Common for consistency.
/// </summary>
public class PrescriptionCacheService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<PrescriptionCacheService> _logger;

    public PrescriptionCacheService(ICacheService cacheService, ILogger<PrescriptionCacheService> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Prescription Caching
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Get prescription from cache by ID.
    /// </summary>
    public async Task<PrescriptionEntity?> GetPrescriptionAsync(Guid prescriptionId)
    {
        var key = $"prescription:rx:{prescriptionId}";
        return await _cacheService.GetAsync<PrescriptionEntity>(key);
    }

    /// <summary>
    /// Cache prescription for 2 hours.
    /// </summary>
    public async Task SetPrescriptionAsync(PrescriptionEntity prescription)
    {
        var key = $"prescription:rx:{prescription.Id}";
        await _cacheService.SetAsync(key, prescription, TimeSpan.FromHours(2));
        _logger.LogDebug($"Cached prescription {prescription.Id}");
    }

    /// <summary>
    /// Invalidate prescription cache.
    /// </summary>
    public async Task InvalidatePrescriptionAsync(Guid prescriptionId)
    {
        var key = $"prescription:rx:{prescriptionId}";
        await _cacheService.RemoveAsync(key);
        _logger.LogDebug($"Invalidated cache for prescription {prescriptionId}");
    }

    /// <summary>
    /// Get all prescriptions for patient (cached).
    /// </summary>
    public async Task<List<PrescriptionEntity>?> GetPatientPrescriptionsAsync(Guid patientId)
    {
        var key = $"prescription:patient:{patientId}";
        return await _cacheService.GetAsync<List<PrescriptionEntity>>(key);
    }

    /// <summary>
    /// Cache all prescriptions for patient (1 hour).
    /// </summary>
    public async Task SetPatientPrescriptionsAsync(Guid patientId, List<PrescriptionEntity> prescriptions)
    {
        var key = $"prescription:patient:{patientId}";
        await _cacheService.SetAsync(key, prescriptions, TimeSpan.FromHours(1));
        _logger.LogDebug($"Cached {prescriptions.Count} prescriptions for patient {patientId}");
    }

    /// <summary>
    /// Invalidate all prescriptions cache for patient.
    /// </summary>
    public async Task InvalidatePatientPrescriptionsAsync(Guid patientId)
    {
        var key = $"prescription:patient:{patientId}";
        await _cacheService.RemoveAsync(key);
        _logger.LogDebug($"Invalidated prescriptions cache for patient {patientId}");
    }

    /// <summary>
    /// Get active prescriptions for patient (cached, 30 min - frequently accessed).
    /// </summary>
    public async Task<List<PrescriptionEntity>?> GetPatientActivePrescriptionsAsync(Guid patientId)
    {
        var key = $"prescription:active:{patientId}";
        return await _cacheService.GetAsync<List<PrescriptionEntity>>(key);
    }

    /// <summary>
    /// Cache active prescriptions for patient (30 minutes).
    /// </summary>
    public async Task SetPatientActivePrescriptionsAsync(Guid patientId, List<PrescriptionEntity> activePrescriptions)
    {
        var key = $"prescription:active:{patientId}";
        await _cacheService.SetAsync(key, activePrescriptions, TimeSpan.FromMinutes(30));
        _logger.LogDebug($"Cached {activePrescriptions.Count} active prescriptions for patient {patientId}");
    }

    /// <summary>
    /// Invalidate active prescriptions cache.
    /// </summary>
    public async Task InvalidatePatientActivePrescriptionsAsync(Guid patientId)
    {
        var key = $"prescription:active:{patientId}";
        await _cacheService.RemoveAsync(key);
        _logger.LogDebug($"Invalidated active prescriptions cache for patient {patientId}");
    }

    // ─────────────────────────────────────────────────────────────────────
    // Refill Requests Caching
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Get refill request from cache by ID.
    /// </summary>
    public async Task<PrescriptionRefill?> GetRefillRequestAsync(Guid refillId)
    {
        var key = $"prescription:refill:{refillId}";
        return await _cacheService.GetAsync<PrescriptionRefill>(key);
    }

    /// <summary>
    /// Cache refill request for 1 hour.
    /// </summary>
    public async Task SetRefillRequestAsync(PrescriptionRefill refillRequest)
    {
        var key = $"prescription:refill:{refillRequest.Id}";
        await _cacheService.SetAsync(key, refillRequest, TimeSpan.FromHours(1));
        _logger.LogDebug($"Cached refill request {refillRequest.Id}");
    }

    /// <summary>
    /// Invalidate refill request cache.
    /// </summary>
    public async Task InvalidateRefillRequestAsync(Guid refillId)
    {
        var key = $"prescription:refill:{refillId}";
        await _cacheService.RemoveAsync(key);
        _logger.LogDebug($"Invalidated cache for refill request {refillId}");
    }

    /// <summary>
    /// Get pending refill requests for patient (cached, 15 min - high priority).
    /// </summary>
    public async Task<List<PrescriptionRefill>?> GetPatientPendingRefillsAsync(Guid patientId)
    {
        var key = $"prescription:refills:pending:{patientId}";
        return await _cacheService.GetAsync<List<PrescriptionRefill>>(key);
    }

    /// <summary>
    /// Cache pending refill requests for patient (15 minutes).
    /// </summary>
    public async Task SetPatientPendingRefillsAsync(Guid patientId, List<PrescriptionRefill> pendingRefills)
    {
        var key = $"prescription:refills:pending:{patientId}";
        await _cacheService.SetAsync(key, pendingRefills, TimeSpan.FromMinutes(15));
        _logger.LogDebug($"Cached {pendingRefills.Count} pending refill requests for patient {patientId}");
    }

    /// <summary>
    /// Invalidate pending refill requests cache.
    /// </summary>
    public async Task InvalidatePatientPendingRefillsAsync(Guid patientId)
    {
        var key = $"prescription:refills:pending:{patientId}";
        await _cacheService.RemoveAsync(key);
        _logger.LogDebug($"Invalidated pending refills cache for patient {patientId}");
    }

    /// <summary>
    /// Get refills for prescription (cached).
    /// </summary>
    public async Task<List<PrescriptionRefill>?> GetPrescriptionRefillsAsync(Guid prescriptionId)
    {
        var key = $"prescription:rx:refills:{prescriptionId}";
        return await _cacheService.GetAsync<List<PrescriptionRefill>>(key);
    }

    /// <summary>
    /// Cache refills for prescription (1 hour).
    /// </summary>
    public async Task SetPrescriptionRefillsAsync(Guid prescriptionId, List<PrescriptionRefill> refills)
    {
        var key = $"prescription:rx:refills:{prescriptionId}";
        await _cacheService.SetAsync(key, refills, TimeSpan.FromHours(1));
        _logger.LogDebug($"Cached {refills.Count} refills for prescription {prescriptionId}");
    }

    /// <summary>
    /// Invalidate prescription refills cache.
    /// </summary>
    public async Task InvalidatePrescriptionRefillsAsync(Guid prescriptionId)
    {
        var key = $"prescription:rx:refills:{prescriptionId}";
        await _cacheService.RemoveAsync(key);
        _logger.LogDebug($"Invalidated refills cache for prescription {prescriptionId}");
    }
}

