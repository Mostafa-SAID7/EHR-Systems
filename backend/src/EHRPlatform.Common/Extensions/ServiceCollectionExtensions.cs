using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using Serilog;
using Serilog.Extensions.Logging;
using EHRPlatform.Common.Caching;
using EHRPlatform.Common.Security;
using EHRPlatform.Common.Health;
using EHRPlatform.Common.Behaviors;
using EHRPlatform.Common.Middleware;
using EHRPlatform.Common.CDC;
using EHRPlatform.Common.Slugs;
using EHRPlatform.Common.Tags;
using EHRPlatform.Common.Categories;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace EHRPlatform.Common.Extensions;

/// <summary>
/// Configuration options for EHR Common services.
/// </summary>
public class EHRCommonOptions
{
    /// <summary>Redis connection string (e.g., "localhost:6379,password=secret").</summary>
    public string RedisConnectionString { get; set; } = "localhost:6379";

    /// <summary>Encryption key for sensitive data (must be 32+ characters).</summary>
    public string EncryptionKey { get; set; } = string.Empty;

    /// <summary>Enable or disable Redis caching (default: true).</summary>
    public bool EnableCaching { get; set; } = true;

    /// <summary>Enable or disable encryption (default: true).</summary>
    public bool EnableEncryption { get; set; } = true;

    /// <summary>Enable or disable Serilog logging (default: true).</summary>
    public bool EnableLogging { get; set; } = true;
}

/// <summary>
/// Extension methods for registering EHR Common services.
/// </summary>
public static class ServiceCollectionExtensions
{
    // ── Aggregate "add everything" ────────────────────────────────────────────

    /// <summary>
    /// Add all EHR Common infrastructure services: logging, caching, encryption,
    /// ICurrentUserService, CDC, and MediatR caching behavior.
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
            services.AddCaching(options);

        if (options.EnableEncryption)
            services.AddEncryption(options);

        // Current-user service (HTTP-context scoped)
        services.AddEHRCurrentUser();

        // Slug generation service
        services.AddSlugGeneration();

        // Tag management service
        services.AddTagServices();

        // Tag query service
        services.AddTagQueryService();

        // CDC fan-out service
        services.AddSingleton<ICdcService, OutboxCdcService>();

        // MediatR caching behavior
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(CachingBehavior<,>));

        return services;
    }

    // ── Current User ──────────────────────────────────────────────────────────

    /// <summary>
    /// Register IHttpContextAccessor + HttpContextCurrentUserService.
    /// Call this in every microservice so handlers can read the acting user.
    /// </summary>
    public static IServiceCollection AddEHRCurrentUser(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, HttpContextCurrentUserService>();
        return services;
    }

    // ── Caching ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Add Redis caching. Connection failures throw at startup (fail-fast).
    /// </summary>
    private static IServiceCollection AddCaching(
        this IServiceCollection services,
        EHRCommonOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.RedisConnectionString))
            throw new InvalidOperationException(
                "Redis connection string is required. Set EHRCommon:RedisConnectionString.");

        try
        {
            var connectionMultiplexer = ConnectionMultiplexer.Connect(options.RedisConnectionString);
            services.AddSingleton<IConnectionMultiplexer>(connectionMultiplexer);
            services.AddSingleton<ICacheService, RedisCacheService>();
            services.AddHealthChecks().AddCacheHealthCheck();
            return services;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to connect to Redis at {options.RedisConnectionString}", ex);
        }
    }

    /// <summary>
    /// Add Redis caching from an explicit connection string.
    /// Degrades gracefully — logs a warning and skips Redis if the string is empty.
    /// Use this variant (instead of DataAccessExtensions.AddRedisCaching) when Redis
    /// is optional and the service should start without it.
    /// </summary>
    public static IServiceCollection AddOptionalRedisCaching(
        this IServiceCollection services,
        string connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            Log.Warning("Redis connection string is empty — caching disabled");
            return services;
        }

        try
        {
            var mux = ConnectionMultiplexer.Connect(connectionString);
            services.AddSingleton<IConnectionMultiplexer>(mux);
            services.AddSingleton<ICacheService, RedisCacheService>();
            return services;
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Redis not available ({ConnectionString}) — caching disabled", connectionString);
            return services;
        }
    }

    // ── Encryption ────────────────────────────────────────────────────────────

    private static IServiceCollection AddEncryption(
        this IServiceCollection services,
        EHRCommonOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.EncryptionKey))
            throw new InvalidOperationException(
                "Encryption key is required. Set EHRCommon:EncryptionKey or ENCRYPTION_KEY env var.");

        services.AddSingleton<IEncryptionService>(new EncryptionService(options.EncryptionKey));
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
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
        return services;
    }

    // ── Slug Generation ───────────────────────────────────────────────────────

    /// <summary>
    /// Register slug generation service for URL-friendly entity identifiers.
    /// </summary>
    public static IServiceCollection AddSlugGeneration(this IServiceCollection services)
    {
        services.AddSingleton<ISlugGenerator, SlugGenerator>();
        return services;
    }

    // ── Tag Management ────────────────────────────────────────────────────────

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

    // ── Category Management ───────────────────────────────────────────────────

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

    // ── Logging ───────────────────────────────────────────────────────────────

    /// <summary>Add Serilog structured logging (public so the API Gateway can call it directly).</summary>
    public static IServiceCollection AddSerilogLogging(this IServiceCollection services)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.File(
                "logs/ehr-platform-.txt",
                rollingInterval: RollingInterval.Day,
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
            .Enrich.FromLogContext()
            .CreateLogger();

        services.AddLogging(logBuilder =>
        {
            logBuilder.ClearProviders();
            logBuilder.AddSerilog();
        });

        return services;
    }
}
