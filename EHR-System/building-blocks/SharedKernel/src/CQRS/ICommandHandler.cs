namespace EHRPlatform.SharedKernel.CQRS;

/// <summary>
/// Command handler contract (no return value).
/// Single responsibility: Command handler contract for void commands.
/// </summary>
public interface ICommandHandler<in TCommand> where TCommand : ICommand
{
    /// <summary>
    /// Handle command execution.
    /// </summary>
    Task ExecuteAsync(TCommand command, CancellationToken cancellationToken = default);
}
