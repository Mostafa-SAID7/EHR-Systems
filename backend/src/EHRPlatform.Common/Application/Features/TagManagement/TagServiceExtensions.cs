#nullable enable

using Microsoft.Extensions.DependencyInjection;
using EHRPlatform.Common.Application.Features.TagManagement.Commands;
using EHRPlatform.Common.Application.Features.TagManagement.Handlers;
using EHRPlatform.Common.Application.Features.TagManagement.Services;
using EHRPlatform.Common.Application.Features.TagManagement.Validators;
using EHRPlatform.Common.Shared.Contracts;

namespace EHRPlatform.Common.Application.Features.TagManagement;

/// <summary>
/// Extension methods for registering tag management services.
/// Single responsibility: Manage tag service, query service, commands, and category provider registration.
/// </summary>
public static class TagServiceExtensions
{
    /// <summary>
    /// Register tag service for centralized tagging infrastructure.
    /// </summary>
    public static IServiceCollection AddTagServices(this IServiceCollection services)
    {
        services.AddSingleton<ITagService, TagService>();
        return services;
    }

    /// <summary>
    /// Register tag query service for advanced tag searching and filtering.
    /// </summary>
    public static IServiceCollection AddTagQueryService(this IServiceCollection services)
    {
        services.AddScoped<ITagQueryService, TagQueryService>();
        return services;
    }

    /// <summary>
    /// Register tag management commands, handlers, and validators.
    /// Includes: ApplyTagsCommand, RemoveTagCommand, SetResourceTagsCommand.
    /// </summary>
    public static IServiceCollection AddTagManagementCommands(this IServiceCollection services)
    {
        services.AddScoped<ITagAssignmentValidator, TagAssignmentValidator>();
        services.AddScoped<ICommandHandler<ApplyTagsCommand, TagAssignmentResponse>, ApplyTagsCommandHandler>();
        services.AddScoped<ICommandHandler<RemoveTagCommand, TagAssignmentResponse>, RemoveTagCommandHandler>();
        services.AddScoped<ICommandHandler<SetResourceTagsCommand, TagAssignmentResponse>, SetResourceTagsCommandHandler>();
        return services;
    }

    /// <summary>
    /// Register category providers for service-specific categorization logic.
    /// Call this from each microservice's Program.cs to enable centralized tagging.
    /// </summary>
    public static IServiceCollection AddCategoryProviders(
        this IServiceCollection services,
        params Type[] providerTypes)
    {
        // Register all provided ICategoryProvider implementations
        foreach (var providerType in providerTypes)
        {
            if (!typeof(ICategoryProvider).IsAssignableFrom(providerType))
                throw new InvalidOperationException(
                    $"Type {providerType.Name} does not implement ICategoryProvider");

            services.AddScoped(typeof(ICategoryProvider), providerType);
        }

        return services;
    }

    /// <summary>
    /// Convenience overload: register a single category provider.
    /// </summary>
    public static IServiceCollection AddCategoryProvider<T>(this IServiceCollection services)
        where T : class, ICategoryProvider
    {
        services.AddScoped<ICategoryProvider, T>();
        return services;
    }
}
