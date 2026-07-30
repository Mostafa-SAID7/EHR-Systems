using EHRPlatform.Common.Data;
using EHRPlatform.Common.Infrastructure.Health;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace EHRPlatform.Common.Shared.Extensions;

/// <summary>
/// DI extension methods for MongoDB data access.
///
/// Typical usage in a microservice Program.cs:
/// <code>
/// builder.Services.AddMongoDataAccess(
///     connectionString: builder.Configuration["MongoDB:ConnectionString"],
///     databaseName:     builder.Configuration["MongoDB:DatabaseName"]);
/// </code>
///
/// Then inject <see cref="IMongoRepository{TDocument}"/> directly, or register
/// a concrete typed repository:
/// <code>
/// services.AddScoped&lt;IMongoRepository&lt;ClinicalNote&gt;, MongoRepository&lt;ClinicalNote&gt;&gt;();
/// </code>
/// </summary>
public static class MongoExtensions
{
    /// <summary>
    /// Register MongoDB client, database, and the generic MongoRepository factory.
    /// Adds a health check for the database connectivity.
    /// </summary>
    public static IServiceCollection AddMongoDataAccess(
        this IServiceCollection services,
        string? connectionString,
        string? databaseName)
    {
        if (string.IsNullOrEmpty(connectionString))
            throw new ArgumentException("MongoDB connection string is required.", nameof(connectionString));
        if (string.IsNullOrEmpty(databaseName))
            throw new ArgumentException("MongoDB database name is required.", nameof(databaseName));

        // Singleton MongoClient — driver manages connection pooling internally.
        services.AddSingleton<IMongoClient>(_ =>
        {
            var settings = MongoClientSettings.FromConnectionString(connectionString);
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(10);
            settings.ConnectTimeout         = TimeSpan.FromSeconds(10);
            return new MongoClient(settings);
        });

        // Scoped IMongoDatabase — cheap handle, no real connection per scope.
        services.AddScoped<IMongoDatabase>(sp =>
            sp.GetRequiredService<IMongoClient>().GetDatabase(databaseName));

        // Generic repository — resolve IMongoRepository<T> or inherit MongoRepository<T>.
        services.AddScoped(typeof(IMongoRepository<>), typeof(MongoRepository<>));

        // Health check
        services.AddHealthChecks()
            .AddMongoHealthCheck();

        return services;
    }
}

