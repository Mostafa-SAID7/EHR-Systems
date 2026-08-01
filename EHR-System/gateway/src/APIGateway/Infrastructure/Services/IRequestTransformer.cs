namespace EHRPlatform.Gateway.Infrastructure.Services;

/// <summary>
/// Service for transforming external API requests to internal microservice formats.
/// Handles DTO mapping and schema conversion.
/// </summary>
public interface IRequestTransformer
{
    T TransformRequest<T>(object request) where T : class;
    object TransformResponse<TService>(TService serviceResponse) where TService : class;
}
