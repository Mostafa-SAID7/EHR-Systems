#nullable enable

namespace EHRPlatform.Common.Application.CQRS;

/// <summary>
/// Marker interface for queries that return a specific result type.
/// Queries are read-only operations and should not modify state.
/// </summary>
/// <typeparam name="TResult">The result type returned by the query.</typeparam>
public interface IQuery<out TResult> : MediatR.IRequest<TResult>
{
}

