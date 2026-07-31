namespace EHRPlatform.Common.Infrastructure.Caching.Handlers;

/// <summary>
/// Abstraction for domain-specific cache invalidation.
/// Single responsibility: Define cache invalidation contract.
/// </summary>
public interface ICacheInvalidationHandler
{
    /// <summary>
    /// Handle event and invalidate related caches.
    /// </summary>
    Task HandleEventAsync(string eventType, dynamic eventData, CancellationToken cancellationToken = default);
}
