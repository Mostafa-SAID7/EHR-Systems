#nullable enable

using Microsoft.Extensions.DependencyInjection;
using EHRPlatform.Common.Slugs;

namespace EHRPlatform.Common.Application.Common.Extensions;

/// <summary>
/// Extension methods for registering slug generation services.
/// Single responsibility: Manage slug generator registration.
/// </summary>
public static class SlugGenerationExtensions
{
    /// <summary>
    /// Register slug generation service for URL-friendly entity identifiers.
    /// </summary>
    public static IServiceCollection AddSlugGeneration(this IServiceCollection services)
    {
        services.AddSingleton<ISlugGenerator, SlugGenerator>();
        return services;
    }
}
