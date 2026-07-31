using Microsoft.Extensions.Logging;

namespace EHRPlatform.Common.Infrastructure.Caching.Handlers;

/// <summary>
/// Handles appointment-related cache invalidation.
/// Single responsibility: Invalidate only appointment caches.
/// </summary>
public class AppointmentCacheInvalidationHandler : ICacheInvalidationHandler
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<AppointmentCacheInvalidationHandler> _logger;

    public AppointmentCacheInvalidationHandler(ICacheService cacheService, ILogger<AppointmentCacheInvalidationHandler> logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleEventAsync(string eventType, dynamic eventData, CancellationToken cancellationToken = default)
    {
        try
        {
            var appointmentId = (Guid)eventData.AppointmentId;
            Guid? patientId = eventData.PatientId;
            Guid? providerId = eventData.ProviderId;

            switch (eventType)
            {
                case "AppointmentCreated":
                case "AppointmentUpdated":
                case "AppointmentCancelled":
                    await _cacheService.RemoveAsync(
                        CacheKeyGenerator.AppointmentKey(appointmentId),
                        cancellationToken);

                    if (patientId.HasValue && patientId != Guid.Empty)
                    {
                        await _cacheService.RemoveAsync(
                            CacheKeyGenerator.AppointmentsByPatientKey(patientId.Value),
                            cancellationToken);
                    }

                    if (providerId.HasValue && providerId != Guid.Empty)
                    {
                        await _cacheService.RemoveByPatternAsync(
                            $"appointments:provider:{providerId}:*",
                            cancellationToken);
                    }

                    _logger.LogInformation("Invalidated appointment caches - Event: {EventType}, AppointmentId: {AppointmentId}",
                        eventType, appointmentId);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error handling appointment cache invalidation: {EventType}", eventType);
        }
    }
}
