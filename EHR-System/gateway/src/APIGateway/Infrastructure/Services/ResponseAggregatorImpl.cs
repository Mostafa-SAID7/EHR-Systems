namespace EHRPlatform.Gateway.Infrastructure.Services;

/// <summary>
/// Implementation of response aggregation from multiple microservices.
/// </summary>
public class ResponseAggregator : IResponseAggregator
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ResponseAggregator> _logger;

    public ResponseAggregator(HttpClient httpClient, ILogger<ResponseAggregator> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<T> AggregateAsync<T>(params (string ServiceUrl, string Endpoint)[] calls) 
        where T : class, new()
    {
        try
        {
            // Make all calls in parallel
            var tasks = calls.Select(call =>
                _httpClient.GetAsync($"{call.ServiceUrl}{call.Endpoint}")
                    .ContinueWith(async t =>
                    {
                        if (t.IsCompletedSuccessfully)
                        {
                            return await t.Result.Content.ReadAsStringAsync();
                        }
                        return null;
                    })
            ).ToArray();

            var results = await Task.WhenAll(tasks);

            _logger.LogInformation("Aggregated responses from {Count} services", results.Length);

            // Combine results - implementation depends on T structure
            var aggregated = new T();
            return aggregated;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error aggregating responses");
            throw;
        }
    }
}
