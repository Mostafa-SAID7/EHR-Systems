namespace EHRPlatform.Gateway.Infrastructure.Services;

using System.Text.Json;

/// <summary>
/// Service for transforming external API requests to internal microservice formats.
/// Handles DTO mapping and schema conversion.
/// </summary>
public interface IRequestTransformer
{
    T TransformRequest<T>(object request) where T : class;
    object TransformResponse<TService>(TService serviceResponse) where TService : class;
}

public class RequestTransformer : IRequestTransformer
{
    private readonly ILogger<RequestTransformer> _logger;
    private static readonly JsonSerializerOptions JsonOptions = 
        new() { PropertyNameCaseInsensitive = true };

    public RequestTransformer(ILogger<RequestTransformer> logger)
    {
        _logger = logger;
    }

    public T TransformRequest<T>(object request) where T : class
    {
        try
        {
            var json = JsonSerializer.Serialize(request, JsonOptions);
            var transformed = JsonSerializer.Deserialize<T>(json, JsonOptions);
            
            _logger.LogDebug("Transformed request to {TargetType}", typeof(T).Name);
            
            return transformed ?? throw new InvalidOperationException("Transformation resulted in null");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transforming request to {TargetType}", typeof(T).Name);
            throw;
        }
    }

    public object TransformResponse<TService>(TService serviceResponse) where TService : class
    {
        // This would contain logic to transform service responses back to external format
        _logger.LogDebug("Transformed response from {SourceType}", typeof(TService).Name);
        return serviceResponse;
    }
}
