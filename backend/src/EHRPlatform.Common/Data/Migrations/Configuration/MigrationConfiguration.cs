#nullable enable

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EHRPlatform.Common.Data.Migrations.Configuration;

/// <summary>
/// Migration configuration builder for fluent API.
/// Single responsibility: Build and configure migration strategies for DbContexts.
/// </summary>
public class MigrationConfiguration
{
    private readonly IServiceCollection _services;
    private string _environment = "development";

    public MigrationConfiguration(IServiceCollection services)
    {
        _services = services;
    }

    /// <summary>
    /// Set the environment (development/staging/production).
    /// </summary>
    public MigrationConfiguration WithEnvironment(string environment)
    {
        _environment = environment;
        return this;
    }

    /// <summary>
    /// Register a DbContext for migrations.
    /// </summary>
    public MigrationConfiguration AddContext<TContext>() where TContext : DbContext
    {
        _services.AddMigrationStrategyByEnvironment<TContext>(_environment);
        return this;
    }

    /// <summary>
    /// Register multiple DbContexts for migrations.
    /// </summary>
    public MigrationConfiguration AddContexts(params Type[] contextTypes)
    {
        foreach (var contextType in contextTypes)
        {
            if (!typeof(DbContext).IsAssignableFrom(contextType))
            {
                throw new ArgumentException($"{contextType.Name} is not a DbContext");
            }

            var method = typeof(MigrationStrategies)
                .GetMethod("AddMigrationStrategyByEnvironment")!
                .MakeGenericMethod(contextType);

            method.Invoke(null, new object[] { _services, _environment });
        }

        return this;
    }

    /// <summary>
    /// Build the configuration and return services.
    /// </summary>
    public IServiceCollection Build()
    {
        return _services;
    }
}

/// <summary>
/// Extension methods to configure migrations fluently in Program.cs.
/// </summary>
public static class MigrationConfigurationExtensions
{
    /// <summary>
    /// Create a new migration configuration builder.
    /// Example:
    /// new MigrationConfiguration(services)
    ///     .WithEnvironment(app.Environment.EnvironmentName)
    ///     .AddContexts(typeof(PatientContext), typeof(BillingContext))
    ///     .Build();
    /// </summary>
    public static MigrationConfiguration ConfigureMigrations(
        this IServiceCollection services,
        string environment)
    {
        return new MigrationConfiguration(services).WithEnvironment(environment);
    }
}
