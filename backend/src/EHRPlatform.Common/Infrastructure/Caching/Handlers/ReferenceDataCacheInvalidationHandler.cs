using Microsoft.Extensions.Logging;

namespace EHRPlatform.Common.Infrastructure.Caching.Handlers;

/// <summary>
/// Handles reference data cache invalidation (code tables, lookups).
/// Single responsibility: Invalidate only reference data caches.
/// </summary>
public class ReferenceDataCacheInvalidationHandler : ICacheInvalidationHandler
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<ReferenceDataCacheInvalidationHandler> _logger;

    public ReferenceDataCacheInvalidationHandler(ICacheService cacheService, ILogger<ReferenceDataCacheInvalidationHandler> logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleEventAsync(string eventType, dynamic eventData, CancellationToken cancellationToken = default)
    {
        try
        {
            var dataType = (string)eventData.DataType;

            await _cacheService.RemoveAsync(
                CacheKeyGenerator.ReferenceDataKey(dataType),
                cancellationToken);

            _logger.LogInformation("Invalidated reference data cache - DataType: {DataType}", dataType);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error handling reference data cache invalidation: {EventType}", eventType);
        }
    }
}
