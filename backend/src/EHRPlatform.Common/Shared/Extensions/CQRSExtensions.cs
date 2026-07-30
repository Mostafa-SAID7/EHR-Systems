using System.Reflection;
using EHRPlatform.Common.Application.Behaviors;
using EHRPlatform.Common.Application.CQRS;
using FluentValidation;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EHRPlatform.Common.Shared.Extensions;

/// <summary>
/// DI extensions for registering CQRS infrastructure (MediatR, validators, pipeline behaviors).
/// </summary>
public static class CQRSExtensions
{
    /// <summary>
    /// Register MediatR handlers, FluentValidation validators, and all pipeline behaviors
    /// from the specified assemblies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="assembliesToScan">Assemblies to scan for handlers and validators.</param>
    public static IServiceCollection AddCQRS(
        this IServiceCollection services,
        params Assembly[] assembliesToScan)
    {
        if (assembliesToScan.Length == 0)
            throw new ArgumentException(
                "At least one assembly must be provided.", nameof(assembliesToScan));

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assembliesToScan));
        services.AddValidatorsFromAssemblies(assembliesToScan);

        // Pipeline behaviors execute in registration order: Logging → Validation → Caching → Transaction
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        // Dispatcher facades over IMediator
        services.AddTransient<ICommandDispatcher, MediatRCommandDispatcher>();
        services.AddTransient<IQueryDispatcher, MediatRQueryDispatcher>();

        return services;
    }

    /// <summary>
    /// Register CQRS handlers from the calling assembly only.
    /// Useful when a microservice calls this from its own Program.cs.
    /// </summary>
    public static IServiceCollection AddCQRSFromCurrentAssembly(this IServiceCollection services)
    {
        var callerAssembly = Assembly.GetCallingAssembly();
        return services.AddCQRS(callerAssembly);
    }

    /// <summary>
    /// Register CQRS handlers from assemblies identified by name.
    /// </summary>
    public static IServiceCollection AddCQRSFromAssemblyNames(
        this IServiceCollection services,
        params string[] assemblyNames)
    {
        var assemblies = assemblyNames.Select(Assembly.Load).ToArray();
        return services.AddCQRS(assemblies);
    }
}

