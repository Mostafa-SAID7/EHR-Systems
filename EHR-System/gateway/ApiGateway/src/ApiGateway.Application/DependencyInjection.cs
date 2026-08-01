using Microsoft.Extensions.DependencyInjection;

namespace EHRPlatform.Services.ApiGateway.Application;

/// <summary>
/// Dependency Injection configuration for API Gateway Application Layer
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Registers API Gateway application services
    /// </summary>
    public static IServiceCollection AddApiGatewayApplicationServices(
        this IServiceCollection services)
    {
        // MediatR registration for CQRS routing queries
        // AutoMapper registration for request/response transformation
        
        return services;
    }
}
