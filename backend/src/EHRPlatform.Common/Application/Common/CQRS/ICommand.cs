#nullable enable

namespace EHRPlatform.Common.Application.Common.CQRS;

/// <summary>
/// Marker interface for commands that don't return a value.
/// </summary>
public interface ICommand : MediatR.IRequest
{
}

/// <summary>
/// Marker interface for commands that return a specific result type.
/// </summary>
/// <typeparam name="TResult">The result type returned by the command.</typeparam>
public interface ICommand<out TResult> : MediatR.IRequest<TResult>
{
}

