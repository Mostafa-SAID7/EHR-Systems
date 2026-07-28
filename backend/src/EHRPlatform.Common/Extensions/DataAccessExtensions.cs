using Elastic.Clients.Elasticsearch;
using EHRPlatform.Common.Caching;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Search;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace EHRPlatform.Common.Extensions;

/// <summary>
/// DI extensions for the data-access layer: EF Core, repositories, unit of work,
/// Redis caching, and Elasticsearch search.
///
/// Typical microservice Program.cs usage:
/// <code>
/// builder.Services
///     .AddPostgresDataAccess&lt;MyDbContext&gt;(connectionString)
///     .AddRedisCaching(redisConnectionString)
///     .AddElasticsearchSearch(elasticsearchUrl);
/// </code>
/// </summary>
public static class DataAccessExtensions
{
    /// <summary>
    /// Register data-access services using a custom <see cref="DbContextOptionsBuilder"/> action.
    /// </summary>
    public static IServiceCollection AddDataAccess<TDbContext>(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> configureOptions)
        where TDbContext : BaseDbContext
    {
        services.AddDbContext<TDbContext>(configureOptions);

        services.AddScoped<IUnitOfWork>(sp =>
            new UnitOfWork(sp.GetRequiredService<TDbContext>()));

        // Dapper façade — reuses the same connection as EF Core.
        services.AddScoped<IDapperContext>(sp =>
            new DapperContext(sp.GetRequiredService<TDbContext>()));

        services.AddScoped<IDatabaseMigrator, DatabaseMigrator<TDbContext>>();

        return services;
    }

    /// <summary>
    /// Register data-access with a PostgreSQL connection string (Npgsql).
    /// </summary>
    public static IServiceCollection AddPostgresDataAccess<TDbContext>(
        this IServiceCollection services,
        string? connectionString)
        where TDbContext : BaseDbContext
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentException("Connection string is required.", nameof(connectionString));

        return services.AddDataAccess<TDbContext>(options =>
            options
                .UseNpgsql(connectionString, npgsql =>
                {
                    npgsql.CommandTimeout(30);
                    // Automatically retry transient failures (network blips, brief
                    // connection pool exhaustion, Postgres restart).
                    npgsql.EnableRetryOnFailure(
                        maxRetryCount:   5,
                        maxRetryDelay:   TimeSpan.FromSeconds(30),
                        errorCodesToAdd: null);
                })
                .EnableDetailedErrors());
    }

    /// <summary>
    /// Register Redis distributed caching.
    /// </summary>
    public static IServiceCollection AddRedisCaching(
        this IServiceCollection services,
        string? connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentException("Redis connection string is required.", nameof(connectionString));

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var opts = ConfigurationOptions.Parse(connectionString);
            opts.AbortOnConnectFail = false;
            opts.ConnectTimeout     = 5_000;
            opts.SyncTimeout        = 5_000;
            return ConnectionMultiplexer.Connect(opts);
        });

        services.AddSingleton<ICacheService, RedisCacheService>();

        return services;
    }

    /// <summary>
    /// Register Elasticsearch search service.
    /// </summary>
    public static IServiceCollection AddElasticsearchSearch(
        this IServiceCollection services,
        string? elasticsearchUrl)
    {
        if (string.IsNullOrEmpty(elasticsearchUrl))
            throw new ArgumentException("Elasticsearch URL is required.", nameof(elasticsearchUrl));

        var settings = new ElasticsearchClientSettings(new Uri(elasticsearchUrl))
            .DisableDirectStreaming()
            .ThrowExceptions();

        services.AddSingleton(new ElasticsearchClient(settings));
        services.AddSingleton<ISearchService, ElasticsearchService>();

        return services;
    }

    /// <summary>
    /// Add a hosted service that runs pending EF Core migrations at startup.
    /// </summary>
    public static IServiceCollection AddMigrationHostedService(this IServiceCollection services)
    {
        services.AddHostedService<DatabaseMigrationHostedService>();
        return services;
    }

    /// <summary>
    /// Register a health check that verifies the given DbContext can connect to its database.
    /// </summary>
    public static IHealthChecksBuilder AddDbContextCheck<TDbContext>(
        this IHealthChecksBuilder builder,
        string name,
        string[]? tags = null)
        where TDbContext : DbContext
    {
        return builder.Add(new HealthCheckRegistration(
            name,
            sp => new DbContextHealthCheck<TDbContext>(sp.GetRequiredService<TDbContext>()),
            failureStatus: null,
            tags: tags));
    }
}

/// <summary>
/// Simple health check that verifies a DbContext can connect by calling CanConnectAsync.
/// </summary>
internal sealed class DbContextHealthCheck<TDbContext> : IHealthCheck
    where TDbContext : DbContext
{
    private readonly TDbContext _context;
    public DbContextHealthCheck(TDbContext context) => _context = context;

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var canConnect = await _context.Database.CanConnectAsync(cancellationToken);
            return canConnect
                ? HealthCheckResult.Healthy()
                : HealthCheckResult.Unhealthy("Database unreachable");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(ex.Message, ex);
        }
    }
}

// ─── Migration support ────────────────────────────────────────────────────────

/// <summary>Applies pending EF Core migrations for a given DbContext.</summary>
public interface IDatabaseMigrator
{
    Task MigrateDatabaseAsync(CancellationToken cancellationToken = default);
}

internal sealed class DatabaseMigrator<TDbContext> : IDatabaseMigrator
    where TDbContext : DbContext
{
    private readonly TDbContext _context;

    public DatabaseMigrator(TDbContext context) => _context = context;

    public async Task MigrateDatabaseAsync(CancellationToken cancellationToken = default)
    {
        await _context.Database.MigrateAsync(cancellationToken);
    }
}

/// <summary>
/// Hosted service that runs all registered <see cref="IDatabaseMigrator"/> instances at startup.
/// </summary>
public sealed class DatabaseMigrationHostedService : IHostedService
{
    private readonly IServiceProvider _sp;

    public DatabaseMigrationHostedService(IServiceProvider sp) => _sp = sp;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        using var scope = _sp.CreateScope();
        foreach (var migrator in scope.ServiceProvider.GetServices<IDatabaseMigrator>())
            await migrator.MigrateDatabaseAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}

// ─── Domain enums (used by EF Core value conversion where needed) ─────────────

/// <summary>Encryption state of a stored value.</summary>
public enum EncryptionStatus { Encrypted, Unencrypted, Partial }

/// <summary>Access level for audit trail classification.</summary>
public enum AccessLevel { None = 0, Audit = 1, Clinical = 2, Administrative = 3, Full = 4 }
