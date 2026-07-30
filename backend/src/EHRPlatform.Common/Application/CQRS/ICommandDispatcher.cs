namespace EHRPlatform.Common.Application.CQRS;

/// <summary>
/// Dispatcher for sending commands through the MediatR pipeline.
/// Provides a facade over IMediator for command operations.
/// </summary>
public interface ICommandDispatcher
{
    /// <summary>
    /// Dispatch a command that does not return a value.
    /// </summary>
    Task SendAsync(ICommand command, CancellationToken cancellationToken = default);

    /// <summary>
    /// Dispatch a command that returns a result.
    /// </summary>
    Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default);
}

