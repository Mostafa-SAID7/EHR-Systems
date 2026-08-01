using EHRPlatform.BuildingBlocks.Observability.Caching;
using EHRPlatform.Services.Clinical.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Clinical.Application.Services;

/// <summary>
/// Distributed cache wrapper for Clinical Service.
/// Caches clinical notes, vital signs, diagnoses, procedures.
/// Uses ICacheService from Common for consistency.
/// </summary>
public class ClinicalCacheService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<ClinicalCacheService> _logger;

    public ClinicalCacheService(ICacheService cacheService, ILogger<ClinicalCacheService> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    // ─────────────────────────────────────────────────────────────────────
    // Clinical Notes Caching
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Get clinical note from cache by ID.
    /// </summary>
    public async Task<ClinicalNote?> GetClinicalNoteAsync(Guid clinicalNoteId)
    {
        var key = $"clinical:note:{clinicalNoteId}";
        return await _cacheService.GetAsync<ClinicalNote>(key);
    }

    /// <summary>
    /// Cache clinical note for 1 hour.
    /// </summary>
    public async Task SetClinicalNoteAsync(ClinicalNote clinicalNote)
    {
        var key = $"clinical:note:{clinicalNote.Id}";
        await _cacheService.SetAsync(key, clinicalNote, TimeSpan.FromHours(1));
        _logger.LogDebug($"Cached clinical note {clinicalNote.Id}");
    }

    /// <summary>
    /// Invalidate clinical note cache.
    /// </summary>
    public async Task InvalidateClinicalNoteAsync(Guid clinicalNoteId)
    {
        var key = $"clinical:note:{clinicalNoteId}";
        await _cacheService.RemoveAsync(key);
        _logger.LogDebug($"Invalidated cache for clinical note {clinicalNoteId}");
    }

    /// <summary>
    /// Get all clinical notes for patient (cached).
    /// </summary>
    public async Task<List<ClinicalNote>?> GetPatientClinicalNotesAsync(Guid patientId)
    {
        var key = $"clinical:notes:patient:{patientId}";
        return await _cacheService.GetAsync<List<ClinicalNote>>(key);
    }

    /// <summary>
    /// Cache all clinical notes for patient (30 minutes).
    /// </summary>
    public async Task SetPatientClinicalNotesAsync(Guid patientId, List<ClinicalNote> notes)
    {
        var key = $"clinical:notes:patient:{patientId}";
        await _cacheService.SetAsync(key, notes, TimeSpan.FromMinutes(30));
        _logger.LogDebug($"Cached {notes.Count} clinical notes for patient {patientId}");
    }

    /// <summary>
    /// Invalidate all clinical notes cache for patient.
    /// </summary>
    public async Task InvalidatePatientClinicalNotesAsync(Guid patientId)
    {
        var key = $"clinical:notes:patient:{patientId}";
        await _cacheService.RemoveAsync(key);
        _logger.LogDebug($"Invalidated clinical notes cache for patient {patientId}");
    }

    // ─────────────────────────────────────────────────────────────────────
    // Vital Signs Caching
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Get vital signs from cache by ID.
    /// </summary>
    public async Task<VitalSigns?> GetVitalSignsAsync(Guid vitalSignsId)
    {
        var key = $"clinical:vitals:{vitalSignsId}";
        return await _cacheService.GetAsync<VitalSigns>(key);
    }

    /// <summary>
    /// Cache vital signs (30 minutes - often updated).
    /// </summary>
    public async Task SetVitalSignsAsync(VitalSigns vitalSigns)
    {
        var key = $"clinical:vitals:{vitalSigns.Id}";
        await _cacheService.SetAsync(key, vitalSigns, TimeSpan.FromMinutes(30));
        _logger.LogDebug($"Cached vital signs {vitalSigns.Id}");
    }

    /// <summary>
    /// Get latest vital signs for patient (cached).
    /// </summary>
    public async Task<VitalSigns?> GetLatestPatientVitalSignsAsync(Guid patientId)
    {
        var key = $"clinical:vitals:latest:{patientId}";
        return await _cacheService.GetAsync<VitalSigns>(key);
    }

    /// <summary>
    /// Cache latest vital signs for patient (15 minutes).
    /// </summary>
    public async Task SetLatestPatientVitalSignsAsync(Guid patientId, VitalSigns vitalSigns)
    {
        var key = $"clinical:vitals:latest:{patientId}";
        await _cacheService.SetAsync(key, vitalSigns, TimeSpan.FromMinutes(15));
        _logger.LogDebug($"Cached latest vital signs for patient {patientId}");
    }

    /// <summary>
    /// Invalidate vital signs cache.
    /// </summary>
    public async Task InvalidateVitalSignsAsync(Guid vitalSignsId, Guid patientId)
    {
        await _cacheService.RemoveAsync($"clinical:vitals:{vitalSignsId}");
        await _cacheService.RemoveAsync($"clinical:vitals:latest:{patientId}");
        _logger.LogDebug($"Invalidated vital signs cache for {vitalSignsId}");
    }

    // ─────────────────────────────────────────────────────────────────────
    // Diagnoses Caching
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Get diagnoses for clinical note (cached).
    /// </summary>
    public async Task<List<ClinicalDiagnosis>?> GetClinicalNoteDiagnosesAsync(Guid clinicalNoteId)
    {
        var key = $"clinical:diagnoses:note:{clinicalNoteId}";
        return await _cacheService.GetAsync<List<ClinicalDiagnosis>>(key);
    }

    /// <summary>
    /// Cache diagnoses for clinical note (1 hour).
    /// </summary>
    public async Task SetClinicalNoteDiagnosesAsync(Guid clinicalNoteId, List<ClinicalDiagnosis> diagnoses)
    {
        var key = $"clinical:diagnoses:note:{clinicalNoteId}";
        await _cacheService.SetAsync(key, diagnoses, TimeSpan.FromHours(1));
        _logger.LogDebug($"Cached {diagnoses.Count} diagnoses for note {clinicalNoteId}");
    }

    /// <summary>
    /// Invalidate diagnoses cache.
    /// </summary>
    public async Task InvalidateClinicalNoteDiagnosesAsync(Guid clinicalNoteId)
    {
        var key = $"clinical:diagnoses:note:{clinicalNoteId}";
        await _cacheService.RemoveAsync(key);
        _logger.LogDebug($"Invalidated diagnoses cache for note {clinicalNoteId}");
    }

    // ─────────────────────────────────────────────────────────────────────
    // Procedures Caching
    // ─────────────────────────────────────────────────────────────────────

    /// <summary>
    /// Get procedures for clinical note (cached).
    /// </summary>
    public async Task<List<ClinicalProcedure>?> GetClinicalNoteProceduresAsync(Guid clinicalNoteId)
    {
        var key = $"clinical:procedures:note:{clinicalNoteId}";
        return await _cacheService.GetAsync<List<ClinicalProcedure>>(key);
    }

    /// <summary>
    /// Cache procedures for clinical note (1 hour).
    /// </summary>
    public async Task SetClinicalNoteProceduresAsync(Guid clinicalNoteId, List<ClinicalProcedure> procedures)
    {
        var key = $"clinical:procedures:note:{clinicalNoteId}";
        await _cacheService.SetAsync(key, procedures, TimeSpan.FromHours(1));
        _logger.LogDebug($"Cached {procedures.Count} procedures for note {clinicalNoteId}");
    }

    /// <summary>
    /// Invalidate procedures cache.
    /// </summary>
    public async Task InvalidateClinicalNoteProceduresAsync(Guid clinicalNoteId)
    {
        var key = $"clinical:procedures:note:{clinicalNoteId}";
        await _cacheService.RemoveAsync(key);
        _logger.LogDebug($"Invalidated procedures cache for note {clinicalNoteId}");
    }
}


