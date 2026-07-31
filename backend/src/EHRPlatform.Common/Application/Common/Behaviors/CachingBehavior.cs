using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Common.Infrastructure.Caching;

namespace EHRPlatform.Common.Application.Common.Behaviors;

/// <summary>
/// Marker interface for queries that should be cached automatically.
/// Implement this in your IQuery classes to enable automatic caching.
/// </summary>
public interface ICachedQuery
{
    /// <summary>
    /// Cache key for this query result.
    /// Should be unique and follow cache key patterns.
    /// </summary>
    string CacheKey { get; }

    /// <summary>
    /// Time-to-live for the cached result.
    /// Return null for no expiration (not recommended).
    /// </summary>
    TimeSpan? Duration { get; }
}

/// <summary>
/// MediatR pipeline behavior that automatically caches query results.
/// Applied to all requests where TRequest implements ICachedQuery.
/// Prevents thundering herd with GetOrSet pattern.
/// </summary>
public class CachingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ICachedQuery
    where TResponse : class
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachingBehavior<TRequest, TResponse>> _logger;

    public CachingBehavior(ICacheService cacheService, ILogger<CachingBehavior<TRequest, TResponse>> logger)
    {
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            // Attempt to get from cache
            var cached = await _cacheService.GetAsync<TResponse>(request.CacheKey, cancellationToken);

            if (cached != null)
            {
                _logger.LogDebug("Cache hit for query: {QueryType} with key: {CacheKey}",
                    typeof(TRequest).Name, request.CacheKey);

                return cached;
            }

            _logger.LogDebug("Cache miss for query: {QueryType} with key: {CacheKey}",
                typeof(TRequest).Name, request.CacheKey);

            // Get result from handler
            var result = await next();

            // Cache result if not null and duration is specified
            if (result != null && request.Duration.HasValue)
            {
                await _cacheService.SetAsync(
                    request.CacheKey,
                    result,
                    request.Duration.Value,
                    cancellationToken);

                _logger.LogDebug("Cached query result: {QueryType} with key: {CacheKey} for {Duration}ms",
                    typeof(TRequest).Name, request.CacheKey, request.Duration.Value.TotalMilliseconds);
            }

            return result;
        }
        catch (Exception ex)
        {
            // Cache failures should not break the application
            // Log and continue with direct query execution
            _logger.LogWarning(ex, "Cache operation failed for query: {QueryType}, executing without cache",
                typeof(TRequest).Name);

            return await next();
        }
    }
}

