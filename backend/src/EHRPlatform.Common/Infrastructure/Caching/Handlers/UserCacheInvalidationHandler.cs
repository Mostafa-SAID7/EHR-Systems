using Microsoft.Extensions.Logging;

namespace EHRPlatform.Common.Infrastructure.Caching.Handlers;

/// <summary>
/// Handles user-related cache invalidation (auth, permissions, roles).
/// Single responsibility: Invalidate only user caches.
/// </summary>
public class UserCacheInvalidationHandler : ICacheInvalidationHandler
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<UserCacheInvalidationHandler> _logger;

    public UserCacheInvalidationHandler(ICacheService cacheService, ILogger<UserCacheInvalidationHandler> logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleEventAsync(string eventType, dynamic eventData, CancellationToken cancellationToken = default)
    {
        try
        {
            var userId = (Guid)eventData.UserId;

            switch (eventType)
            {
                case "UserCreated":
                case "UserUpdated":
                case "UserDeleted":
                    await _cacheService.RemoveAsync(CacheKeyGenerator.UserKey(userId), cancellationToken);
                    await _cacheService.RemoveAsync(CacheKeyGenerator.UserRolesKey(userId), cancellationToken);
                    await _cacheService.RemoveAsync(CacheKeyGenerator.UserPermissionsKey(userId), cancellationToken);
                    break;

                case "RoleAssigned":
                case "RoleRevoked":
                    await _cacheService.RemoveAsync(CacheKeyGenerator.UserRolesKey(userId), cancellationToken);
                    await _cacheService.RemoveAsync(CacheKeyGenerator.UserPermissionsKey(userId), cancellationToken);
                    break;

                case "PermissionChanged":
                    await _cacheService.RemoveAsync(CacheKeyGenerator.UserPermissionsKey(userId), cancellationToken);
                    break;
            }

            _logger.LogInformation("Invalidated user caches - Event: {EventType}, UserId: {UserId}",
                eventType, userId);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Error handling user cache invalidation: {EventType}", eventType);
        }
    }
}
