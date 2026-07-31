using MediatR;

namespace EHRPlatform.Common.Application.Common.CQRS;

/// <summary>
/// MediatR-backed implementation of <see cref="ICommandDispatcher"/>.
/// </summary>
public sealed class MediatRCommandDispatcher : ICommandDispatcher
{
    private readonly IMediator _mediator;

    public MediatRCommandDispatcher(IMediator mediator)
        => _mediator = mediator;

    public Task SendAsync(ICommand command, CancellationToken cancellationToken = default)
        => _mediator.Send(command, cancellationToken);

    public Task<TResult> SendAsync<TResult>(ICommand<TResult> command, CancellationToken cancellationToken = default)
        => _mediator.Send(command, cancellationToken);
}

/// <summary>
/// MediatR-backed implementation of <see cref="IQueryDispatcher"/>.
/// </summary>
public sealed class MediatRQueryDispatcher : IQueryDispatcher
{
    private readonly IMediator _mediator;

    public MediatRQueryDispatcher(IMediator mediator)
        => _mediator = mediator;

    public Task<TResult> QueryAsync<TResult>(IQuery<TResult> query, CancellationToken cancellationToken = default)
        => _mediator.Send(query, cancellationToken);
}

