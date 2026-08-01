namespace EHRPlatform.SharedKernel.CQRS;

/// <summary>
/// Mediator pattern for decoupling command/query execution.
/// Single responsibility: Command/Query dispatcher contract.
/// </summary>
public interface IMediator
{
    /// <summary>
    /// Send command for execution.
    /// </summary>
    Task SendAsync<TCommand>(TCommand command, CancellationToken cancellationToken = default) where TCommand : ICommand;

    /// <summary>
    /// Send command and get result.
    /// </summary>
    Task<TResult> SendAsync<TCommand, TResult>(TCommand command, CancellationToken cancellationToken = default) 
        where TCommand : ICommand<TResult>;

    /// <summary>
    /// Execute query and get result.
    /// </summary>
    Task<TResult> QueryAsync<TQuery, TResult>(TQuery query, CancellationToken cancellationToken = default) 
        where TQuery : IQuery<TResult>;
}
