#nullable enable

using EHRPlatform.BuildingBlocks.Common.Application.Behaviors;
using EHRPlatform.Services.Identity.Application.Identity.Mappers;
using EHRPlatform.Services.Identity.Features.Auth.Handlers;
using EHRPlatform.Services.Identity.Features.Auth.Validation;
using EHRPlatform.Services.Identity.Features.Users.Handlers;
using EHRPlatform.Services.Identity.Features.Users.Validation;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace EHRPlatform.Services.Identity.Application.Identity.Extensions;

/// <summary>
/// Service collection extensions for Identity service registration.
/// Registers all handlers, validators, mappers, and pipeline behaviors.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add identity service CQRS and application services.
    /// </summary>
    public static IServiceCollection AddIdentityServices(this IServiceCollection services)
    {
        // Register MediatR handlers from this assembly
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<LoginCommandHandler>());

        // Register validators from this assembly
        services.AddValidatorsFromAssemblyContaining<LoginCommandValidator>(ServiceLifetime.Scoped);

        // Register mappers
        services.AddScoped<IdentityMapper>();

        // Register pipeline behaviors for CQRS
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        return services;
    }
}


