using EHRPlatform.BuildingBlocks.Observability.Caching;

namespace EHRPlatform.Services.Patient.Application.Services;

public interface IPatientCacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default) where T : class;
    Task<T> GetOrSetAsync<T>(string key, Func<string, Task<T>> loader, TimeSpan? expiry = null, CancellationToken ct = default) where T : class;
    Task InvalidateAsync(string key, CancellationToken ct = default);
    
    Task<T?> GetPatientAsync<T>(Guid patientId, CancellationToken ct = default) where T : class;
    Task SetPatientAsync<T>(Guid patientId, T value, CancellationToken ct = default) where T : class;
    Task<T?> GetPatientAllergiesAsync<T>(Guid patientId, CancellationToken ct = default) where T : class;
    Task SetPatientAllergiesAsync<T>(Guid patientId, T value, CancellationToken ct = default) where T : class;
}

public class PatientCacheService : IPatientCacheService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<PatientCacheService> _logger;
    
    private const string PatientKeyPrefix = "patient:";
    private const string AllergyKeyPrefix = "patient:allergies:";

    public PatientCacheService(ICacheService cacheService, ILogger<PatientCacheService> logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class
    {
        try
        {
            return await _cacheService.GetAsync<T>(key, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache get failed for key {key}", key);
            return default;
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default) where T : class
    {
        try
        {
            await _cacheService.SetAsync(key, value, expiry, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache set failed for key {key}", key);
        }
    }

    public async Task<T> GetOrSetAsync<T>(string key, Func<string, Task<T>> loader, TimeSpan? expiry = null, CancellationToken ct = default) where T : class
    {
        try
        {
            return await _cacheService.GetOrSetAsync(key, loader, expiry, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache get-or-set failed for key {key}, loading directly", key);
            return await loader(key);
        }
    }

    public async Task InvalidateAsync(string key, CancellationToken ct = default)
    {
        try
        {
            await _cacheService.RemoveAsync(key, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Cache invalidate failed for key {key}", key);
        }
    }

    public Task<T?> GetPatientAsync<T>(Guid patientId, CancellationToken ct = default) where T : class
        => GetAsync<T>($"{PatientKeyPrefix}{patientId}", ct);

    public Task SetPatientAsync<T>(Guid patientId, T value, CancellationToken ct = default) where T : class
        => SetAsync($"{PatientKeyPrefix}{patientId}", value, TimeSpan.FromHours(2), ct);

    public Task<T?> GetPatientAllergiesAsync<T>(Guid patientId, CancellationToken ct = default) where T : class
        => GetAsync<T>($"{AllergyKeyPrefix}{patientId}", ct);

    public Task SetPatientAllergiesAsync<T>(Guid patientId, T value, CancellationToken ct = default) where T : class
        => SetAsync($"{AllergyKeyPrefix}{patientId}", value, TimeSpan.FromHours(6), ct);
}


