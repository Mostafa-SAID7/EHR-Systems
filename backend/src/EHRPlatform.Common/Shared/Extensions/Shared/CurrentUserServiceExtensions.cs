#nullable enable

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using EHRPlatform.Common.Shared.Contracts;

namespace EHRPlatform.Common.Shared.Services;

/// <summary>
/// Extension methods for registering current user service.
/// Single responsibility: Manage ICurrentUserService registration.
/// </summary>
public static class CurrentUserServiceExtensions
{
    /// <summary>
    /// Register IHttpContextAccessor and HttpContextCurrentUserService.
    /// Call this in every microservice so handlers can read the acting user.
    /// </summary>
    public static IServiceCollection AddEHRCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();
        return services;
    }
}
