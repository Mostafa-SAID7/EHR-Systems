using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using EHRPlatform.Common.Data.Migrations.Configuration;

namespace EHRPlatform.Common.Data.Migrations;

/// <summary>
/// Environment-specific migration strategies.
/// Configures how migrations are handled per environment.
/// </summary>
public static class MigrationStrategies
{
    /// <summary>
    /// Development strategy: Automatic migrations on startup.
    /// Best for rapid iteration.
    /// WARNING: No production data protection.
    /// </summary>
    public static IServiceCollection AddDevelopmentMigrationStrategy<TContext>(
        this IServiceCollection services) where TContext : DbContext
    {
        services.AddScoped(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<TContext>>();
            return new MigrationExecutor(
                sp.GetRequiredService<ILogger<MigrationExecutor>>(),
                sp,
                MigrationPolicy.AutomaticOnStartup);
        });

        return services;
    }

    /// <summary>
    /// Staging strategy: Manual migrations with status checks.
    /// Migrations applied via scripts before deployment.
    /// Suitable for pre-production validation.
    /// </summary>
    public static IServiceCollection AddStagingMigrationStrategy<TContext>(
        this IServiceCollection services) where TContext : DbContext
    {
        services.AddScoped(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<TContext>>();
            return new MigrationExecutor(
                sp.GetRequiredService<ILogger<MigrationExecutor>>(),
                sp,
                MigrationPolicy.ManualOnly);
        });

        return services;
    }

    /// <summary>
    /// Production strategy: No automatic migrations.
    /// Database must be pre-migrated before deployment.
    /// Highest safety with explicit migration management.
    /// </summary>
    public static IServiceCollection AddProductionMigrationStrategy<TContext>(
        this IServiceCollection services) where TContext : DbContext
    {
        services.AddScoped(sp =>
        {
            var logger = sp.GetRequiredService<ILogger<TContext>>();
            return new MigrationExecutor(
                sp.GetRequiredService<ILogger<MigrationExecutor>>(),
                sp,
                MigrationPolicy.Disabled);
        });

        return services;
    }

    /// <summary>
    /// Select migration strategy based on environment.
    /// Call this once in Program.cs based on ASPNETCORE_ENVIRONMENT.
    /// </summary>
    public static IServiceCollection AddMigrationStrategyByEnvironment<TContext>(
        this IServiceCollection services,
        string environment) where TContext : DbContext
    {
        return environment.ToLowerInvariant() switch
        {
            "development" => services.AddDevelopmentMigrationStrategy<TContext>(),
            "staging" => services.AddStagingMigrationStrategy<TContext>(),
            "production" => services.AddProductionMigrationStrategy<TContext>(),
            _ => throw new InvalidOperationException($"Unknown environment: {environment}")
        };
    }
}

/// <summary>
/// Migration startup initializer.
/// Runs migrations when application starts.
/// Call in Program.cs with app.Services.RunMigrationsAsync<TContext>("ServiceName").
/// </summary>
public static class MigrationInitializer
{
    /// <summary>
    /// Run migrations for a DbContext on startup.
    /// Must be called after building the app but before running it.
    /// Example: await app.Services.RunMigrationsAsync<PatientContext>("PatientService");
    /// </summary>
    public static async Task RunMigrationsAsync<TContext>(
        this IServiceProvider services,
        string serviceName) where TContext : DbContext
    {
        using var scope = services.CreateScope();
        var executor = scope.ServiceProvider.GetRequiredService<MigrationExecutor>();
        var result = await executor.ExecuteAsync<TContext>(serviceName);
        
        if (!result.Success && result.Strategy == MigrationPolicy.AutomaticOnStartup)
        {
            throw new InvalidOperationException(
                $"Failed to migrate {serviceName}: {result.ErrorMessage}");
        }
    }

    /// <summary>
    /// Run migrations for multiple DbContexts.
    /// </summary>
    public static async Task RunMigrationsAsync(
        this IServiceProvider services,
        params (Type contextType, string serviceName)[] contexts)
    {
        using var scope = services.CreateScope();
        var executor = scope.ServiceProvider.GetRequiredService<MigrationExecutor>();

        foreach (var (contextType, serviceName) in contexts)
        {
            var method = typeof(MigrationExecutor)
                .GetMethod("ExecuteAsync")!
                .MakeGenericMethod(contextType);

            var task = (Task<MigrationResult>)method.Invoke(executor, new object[] { serviceName })!;
            var result = await task;

            if (!result.Success && result.Strategy == MigrationPolicy.AutomaticOnStartup)
            {
                throw new InvalidOperationException(
                    $"Failed to migrate {serviceName}: {result.ErrorMessage}");
            }
        }
    }
}
