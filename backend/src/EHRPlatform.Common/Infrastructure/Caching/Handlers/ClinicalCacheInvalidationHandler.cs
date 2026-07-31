using Microsoft.Extensions.Logging;

namespace EHRPlatform.Common.Infrastructure.Caching.Handlers;

/// <summary>
/// Handles clinical data cache invalidation (SOAP notes, vitals, diagnoses, allergies).
/// Single responsibility: Invalidate only clinical caches.
/// </summary>
public class ClinicalCacheInvalidationHandler : ICacheInvalidationHandler
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<ClinicalCacheInvalidationHandler> _logger;

    public ClinicalCacheInvalidationHandler(ICacheService cacheService, ILogger<ClinicalCacheInvalidationHandler> logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleEventAsync(string eventType, dynamic eventData, CancellationToken cancellationToken = default)
    {
        try
        {
            var clinicalDataId = (Guid)eventData.ClinicalDataId;
            var patientId = (Guid)eventData.PatientId;

            switch (eventType)
            {
                case "SoapNoteCreated":
                case "SoapNoteUpdated":
                case "SoapNoteDeleted":
                    await _cacheService.RemoveAsync(
                        CacheKeyGenerator.SoapNoteKey(clinicalDataId),
                        cancellationToken);

                    await _cacheService.RemoveAsync(
                        CacheKeyGenerator.PatientSoapNotesKey(patientId),
                        cancellationToken);
                    break;

                case "VitalsUpdated":
                    await _cacheService.RemoveAsync(
                        CacheKeyGenerator.PatientVitalsKey(patientId),
                        cancellationToken);
                    break;

                case "DiagnosisAdded":
                case "DiagnosisUpdated":
                    await _cacheService.RemoveAsync(
                        CacheKeyGenerator.PatientDiagnosesKey(patientId),
                        cancellationToken);
                    break;

                case "AllergyAdded":
                case "AllergyUpdated":
                    await _cacheService.RemoveAsync(
                        CacheKeyGenerator.PatientAllergiesKey(patientId),
                        cancellationToken);
                    break;
            }

            _logger.LogInformation("Invalidated clinical caches - Event: {EventType}, PatientId: {PatientId}",
                eventType, patientId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error handling clinical cache invalidation: {EventType}", eventType);
        }
    }
}
