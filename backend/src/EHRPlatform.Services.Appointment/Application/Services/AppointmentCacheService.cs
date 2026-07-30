using EHRPlatform.Common.Infrastructure.Caching;

namespace EHRPlatform.Services.Appointment.Application.Services;

public interface IAppointmentCacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default) where T : class;
    Task<T> GetOrSetAsync<T>(string key, Func<string, Task<T>> loader, TimeSpan? expiry = null, CancellationToken ct = default) where T : class;
    Task InvalidateAsync(string key, CancellationToken ct = default);
    
    Task<T?> GetAppointmentAsync<T>(Guid appointmentId, CancellationToken ct = default) where T : class;
    Task SetAppointmentAsync<T>(Guid appointmentId, T value, CancellationToken ct = default) where T : class;
    Task<T?> GetProviderAvailabilityAsync<T>(Guid providerId, CancellationToken ct = default) where T : class;
    Task SetProviderAvailabilityAsync<T>(Guid providerId, T value, CancellationToken ct = default) where T : class;
    Task<T?> GetPatientAppointmentsAsync<T>(Guid patientId, CancellationToken ct = default) where T : class;
    Task SetPatientAppointmentsAsync<T>(Guid patientId, T value, CancellationToken ct = default) where T : class;
}

public class AppointmentCacheService : IAppointmentCacheService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<AppointmentCacheService> _logger;
    
    private const string AppointmentKeyPrefix = "appointment:";
    private const string AvailabilityKeyPrefix = "availability:provider:";
    private const string PatientApptKeyPrefix = "patient:appointments:";

    public AppointmentCacheService(ICacheService cacheService, ILogger<AppointmentCacheService> logger)
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

    public Task<T?> GetAppointmentAsync<T>(Guid appointmentId, CancellationToken ct = default) where T : class
        => GetAsync<T>($"{AppointmentKeyPrefix}{appointmentId}", ct);

    public Task SetAppointmentAsync<T>(Guid appointmentId, T value, CancellationToken ct = default) where T : class
        => SetAsync(key: $"{AppointmentKeyPrefix}{appointmentId}", value, TimeSpan.FromHours(1), ct);

    public Task<T?> GetProviderAvailabilityAsync<T>(Guid providerId, CancellationToken ct = default) where T : class
        => GetAsync<T>($"{AvailabilityKeyPrefix}{providerId}", ct);

    public Task SetProviderAvailabilityAsync<T>(Guid providerId, T value, CancellationToken ct = default) where T : class
        => SetAsync($"{AvailabilityKeyPrefix}{providerId}", value, TimeSpan.FromHours(4), ct);

    public Task<T?> GetPatientAppointmentsAsync<T>(Guid patientId, CancellationToken ct = default) where T : class
        => GetAsync<T>($"{PatientApptKeyPrefix}{patientId}", ct);

    public Task SetPatientAppointmentsAsync<T>(Guid patientId, T value, CancellationToken ct = default) where T : class
        => SetAsync($"{PatientApptKeyPrefix}{patientId}", value, TimeSpan.FromHours(2), ct);
}

