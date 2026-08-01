namespace EHRPlatform.Gateway.Infrastructure.Services;

/// <summary>
/// Service for aggregating responses from multiple microservices.
/// Handles parallel calls, timeout, and error handling.
/// </summary>
public interface IResponseAggregator
{
    Task<T> AggregateAsync<T>(params (string ServiceUrl, string Endpoint)[] calls) 
        where T : class, new();
}
