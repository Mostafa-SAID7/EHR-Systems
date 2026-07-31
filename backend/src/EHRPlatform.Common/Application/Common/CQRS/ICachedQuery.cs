namespace EHRPlatform.Common.Application.Common.CQRS;

/// <summary>
/// Marker interface for queries whose results should be cached.
/// Extends <see cref="IQuery{TResult}"/> so handlers typed as
/// <see cref="IQueryHandler{TQuery,TResult}"/> can bind to cached queries.
/// </summary>
/// <typeparam name="TResult">The result type returned by the query.</typeparam>
public interface ICachedQuery<out TResult> : IQuery<TResult>
{
    /// <summary>Unique cache key for this query instance.</summary>
    string CacheKey { get; }

    /// <summary>How long (seconds) to cache the result. 0 = never cache.</summary>
    int CacheDurationSeconds { get; }
}

