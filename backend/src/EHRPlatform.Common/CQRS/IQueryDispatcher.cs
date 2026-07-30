namespace EHRPlatform.Common.CQRS;

/// <summary>
/// Dispatcher for sending queries through the MediatR pipeline.
/// Provides a facade over IMediator for query operations.
/// </summary>
public interface IQueryDispatcher
{
    /// <summary>
    /// Dispatch a query and return its result.
    /// </summary>
    Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default);
}
