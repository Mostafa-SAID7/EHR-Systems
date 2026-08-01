namespace EHRPlatform.SharedKernel.CQRS;

/// <summary>
/// Query handler contract.
/// Single responsibility: Query handler contract for read operations.
/// </summary>
public interface IQueryHandler<in TQuery, TResult> where TQuery : IQuery<TResult>
{
    /// <summary>
    /// Handle query and return result.
    /// </summary>
    Task<TResult> ExecuteAsync(TQuery query, CancellationToken cancellationToken = default);
}
