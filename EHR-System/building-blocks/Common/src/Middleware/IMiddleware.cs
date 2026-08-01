namespace EHRPlatform.Common.Middleware;

/// <summary>
/// Request/response pipeline middleware interface.
/// Single responsibility: Middleware pipeline contract.
/// </summary>
public interface IMiddleware<TRequest, TResponse>
{
    /// <summary>
    /// Execute middleware.
    /// </summary>
    Task<TResponse> ExecuteAsync(TRequest request, Func<TRequest, Task<TResponse>> next);
}
