namespace EHRPlatform.SharedKernel.CQRS;

/// <summary>
/// Typed command handler returning a result.
/// Single responsibility: Command handler contract with return value.
/// </summary>
public interface ICommandHandler<in TCommand, TResult> where TCommand : ICommand<TResult>
{
    /// <summary>
    /// Handle command and return result.
    /// </summary>
    Task<TResult> ExecuteAsync(TCommand command, CancellationToken cancellationToken = default);
}
