using EHRPlatform.Common.Caching;

namespace EHRPlatform.Services.Identity.Application.Services;

public interface IIdentityCacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default) where T : class;
    Task InvalidateAsync(string key, CancellationToken ct = default);
}

public class IdentityCacheService : IIdentityCacheService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<IdentityCacheService> _logger;

    public IdentityCacheService(ICacheService cacheService, ILogger<IdentityCacheService> logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
    {
        try { return await _cacheService.GetAsync<T>(key, ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "Cache get failed for key {key}", key); return default; }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default) where T : class
    {
        try { await _cacheService.SetAsync(key, value, expiry, ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "Cache set failed for key {key}", key); }
    }

    public async Task InvalidateAsync(string key, CancellationToken ct = default)
    {
        try { await _cacheService.RemoveAsync(key, ct); }
        catch (Exception ex) { _logger.LogWarning(ex, "Cache invalidate failed for key {key}", key); }
    }
}
