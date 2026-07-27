using EHRPlatform.Common.Caching;

namespace EHRPlatform.Services.Billing.Application.Services;

/// <summary>
/// Billing-specific caching wrapper over Common's ICacheService.
/// Provides domain-specific cache keys and patterns to prevent key collisions.
/// Reuses 100% of Common's infrastructure (Redis connection, serialization, error handling).
/// </summary>
public interface IBillingCacheService
{
    Task<T?> GetAsync<T>(string key, CancellationToken ct = default) where T : class;
    Task SetAsync<T>(string key, T value, TimeSpan? expiry = null, CancellationToken ct = default) where T : class;
    Task<T> GetOrSetAsync<T>(string key, Func<string, Task<T>> loader, TimeSpan? expiry = null, CancellationToken ct = default) where T : class;
    Task InvalidateAsync(string key, CancellationToken ct = default);
    
    // Billing-specific patterns
    Task<T?> GetInvoiceAsync<T>(Guid invoiceId, CancellationToken ct = default) where T : class;
    Task SetInvoiceAsync<T>(Guid invoiceId, T value, CancellationToken ct = default) where T : class;
    Task<T?> GetPaymentAsync<T>(Guid paymentId, CancellationToken ct = default) where T : class;
    Task SetPaymentAsync<T>(Guid paymentId, T value, CancellationToken ct = default) where T : class;
    Task<T?> GetInsuranceClaimAsync<T>(Guid claimId, CancellationToken ct = default) where T : class;
    Task SetInsuranceClaimAsync<T>(Guid claimId, T value, CancellationToken ct = default) where T : class;
}

public class BillingCacheService : IBillingCacheService
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<BillingCacheService> _logger;
    
    private const string InvoiceKeyPrefix = "billing:invoice:";
    private const string PaymentKeyPrefix = "billing:payment:";
    private const string ClaimKeyPrefix = "billing:claim:";
    private const string DefaultExpiry = "1h"; // 1 hour

    public BillingCacheService(ICacheService cacheService, ILogger<BillingCacheService> logger)
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

    public Task<T?> GetInvoiceAsync<T>(Guid invoiceId, CancellationToken ct = default) where T : class
    {
        var key = $"{InvoiceKeyPrefix}{invoiceId}";
        return GetAsync<T>(key, ct);
    }

    public Task SetInvoiceAsync<T>(Guid invoiceId, T value, CancellationToken ct = default) where T : class
    {
        var key = $"{InvoiceKeyPrefix}{invoiceId}";
        var expiry = TimeSpan.FromHours(1);
        return SetAsync(key, value, expiry, ct);
    }

    public Task<T?> GetPaymentAsync<T>(Guid paymentId, CancellationToken ct = default) where T : class
    {
        var key = $"{PaymentKeyPrefix}{paymentId}";
        return GetAsync<T>(key, ct);
    }

    public Task SetPaymentAsync<T>(Guid paymentId, T value, CancellationToken ct = default) where T : class
    {
        var key = $"{PaymentKeyPrefix}{paymentId}";
        var expiry = TimeSpan.FromHours(2);
        return SetAsync(key, value, expiry, ct);
    }

    public Task<T?> GetInsuranceClaimAsync<T>(Guid claimId, CancellationToken ct = default) where T : class
    {
        var key = $"{ClaimKeyPrefix}{claimId}";
        return GetAsync<T>(key, ct);
    }

    public Task SetInsuranceClaimAsync<T>(Guid claimId, T value, CancellationToken ct = default) where T : class
    {
        var key = $"{ClaimKeyPrefix}{claimId}";
        var expiry = TimeSpan.FromHours(3);
        return SetAsync(key, value, expiry, ct);
    }
}
