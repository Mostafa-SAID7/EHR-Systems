#nullable enable

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using EHRPlatform.Common.Infrastructure.Caching;
using EHRPlatform.Common.Infrastructure.Security;
using EHRPlatform.Common.Infrastructure.Telemetry;
using EHRPlatform.Common.Infrastructure.Options;
using EHRPlatform.Common.Application.Common.Behaviors;
using EHRPlatform.Common.Application.Features.TagManagement;
using EHRPlatform.Common.Application.Common.Extensions;
using EHRPlatform.Common.Shared.Services;
using MediatR;

namespace EHRPlatform.Common.Application.Common.Extensions;

/// <summary>
/// Extension methods for registering EHR Common services.
/// Aggregate extension that delegates to specialized extension files.
/// </summary>
public static class ServiceCollectionExtensions
{
    // ── Aggregate "add everything" ────────────────────────────────────────────

    /// <summary>
    /// Add all EHR Common infrastructure services: logging, caching, encryption,
    /// ICurrentUserService, slug generation, tag management, and MediatR caching behavior.
    /// </summary>
    public static IServiceCollection AddEHRCommon(
        this IServiceCollection services,
        IConfiguration configuration,
        EHRCommonOptions? options = null)
    {
        options ??= new EHRCommonOptions();
        configuration.GetSection("EHRCommon").Bind(options);

        if (options.EnableLogging)
            services.AddSerilogLogging();

        if (options.EnableCaching)
            services.AddRedisCaching(options.RedisConnectionString);

        if (options.EnableEncryption)
            services.AddSecurityServices(options.EncryptionKey);

        // Current-user service (HTTP-context scoped)
        services.AddEHRCurrentUser();

        // Slug generation service
        services.AddSlugGeneration();

        // Tag management services
        services.AddTagServices();
        services.AddTagQueryService();

        // MediatR caching behavior
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

        return services;
    }

    // ── Legacy shim ───────────────────────────────────────────────────────────

    /// <summary>
    /// One-call convenience wrapper for legacy microservice Program.cs files.
    /// Prefer calling the individual extension methods directly in new services.
    /// </summary>
    public static IServiceCollection AddCommonServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddCQRSFromCurrentAssembly();
        services.AddEHRCommon(configuration);
        services.AddTagManagementCommands();
        return services;
    }
}
