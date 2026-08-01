namespace EHRPlatform.Common.Middleware;

/// <summary>
/// Middleware pipeline executor.
/// Single responsibility: Middleware pipeline execution contract.
/// </summary>
public interface IMiddlewarePipeline<TRequest, TResponse>
{
    /// <summary>
    /// Add middleware to pipeline.
    /// </summary>
    void Use(IMiddleware<TRequest, TResponse> middleware);

    /// <summary>
    /// Execute all middleware in pipeline.
    /// </summary>
    Task<TResponse> ExecuteAsync(TRequest request, Func<TRequest, Task<TResponse>> terminalHandler);
}
