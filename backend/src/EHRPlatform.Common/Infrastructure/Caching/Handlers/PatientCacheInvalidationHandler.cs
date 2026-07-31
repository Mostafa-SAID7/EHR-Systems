using Microsoft.Extensions.Logging;

namespace EHRPlatform.Common.Infrastructure.Caching.Handlers;

/// <summary>
/// Handles patient-related cache invalidation.
/// Single responsibility: Invalidate only patient caches.
/// </summary>
public class PatientCacheInvalidationHandler : ICacheInvalidationHandler
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<PatientCacheInvalidationHandler> _logger;

    public PatientCacheInvalidationHandler(ICacheService cacheService, ILogger<PatientCacheInvalidationHandler> logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleEventAsync(string eventType, dynamic eventData, CancellationToken cancellationToken = default)
    {
        try
        {
            var patientId = (Guid)eventData.PatientId;

            switch (eventType)
            {
                case "PatientCreated":
                case "PatientUpdated":
                case "PatientDeleted":
                    await _cacheService.RemoveByPatternAsync(
                        $"patient:{patientId}:*",
                        cancellationToken);

                    await _cacheService.RemoveAsync(CacheKeyGenerator.PatientsListKey, cancellationToken);
                    await _cacheService.RemoveByPatternAsync("patients:paged:*", cancellationToken);
                    await _cacheService.RemoveByPatternAsync("patients:search:*", cancellationToken);

                    _logger.LogInformation("Invalidated patient caches - Event: {EventType}, PatientId: {PatientId}",
                        eventType, patientId);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error handling patient cache invalidation: {EventType}", eventType);
        }
    }
}
