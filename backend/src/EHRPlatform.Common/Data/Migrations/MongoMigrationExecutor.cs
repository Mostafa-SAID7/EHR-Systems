using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;
using EHRPlatform.Common.Shared.Utilities.Helpers;

namespace EHRPlatform.Common.Data.Migrations;

/// <summary>
/// MongoDB migration executor for document-based data.
/// Handles schema versioning, index creation, and data transformation for MongoDB collections.
/// </summary>
public class MongoMigrationExecutor
{
    private readonly ILogger<MongoMigrationExecutor> _logger;
    private readonly IMongoDatabase _database;
    private readonly string _migrationCollectionName = "__MigrationHistory";

    public MongoMigrationExecutor(IMongoDatabase database, ILogger<MongoMigrationExecutor> logger)
    {
        _database = database;
        _logger = logger;
    }

    /// <summary>
    /// Result of a MongoDB migration execution.
    /// </summary>
    public record MongoMigrationResult(
        bool Success,
        string MigrationId,
        DateTime ExecutedAt,
        string? ErrorMessage = null,
        int DocumentsAffected = 0
    );

    /// <summary>
    /// Execute a MongoDB migration script.
    /// Migrations are idempotent — executing twice is safe.
    /// </summary>
    public async Task<MongoMigrationResult> ExecuteAsync(
        string migrationId,
        Func<IMongoDatabase, Task> migrationScript)
    {
        try
        {
            _logger.LogInformation("Starting MongoDB migration: {MigrationId}", migrationId);

            // Check if migration already applied
            var history = _database.GetCollection<MigrationHistoryDocument>(_migrationCollectionName);
            var existing = await history.FindAsync(m => m.MigrationId == migrationId);
            
            if (await existing.AnyAsync())
            {
                _logger.LogWarning("MongoDB migration already applied: {MigrationId}", migrationId);
                return new MongoMigrationResult(
                    Success: true,
                    MigrationId: migrationId,
                    ExecutedAt: DateTimeHelper.UtcNow,
                    ErrorMessage: "Already applied"
                );
            }

            // Execute migration script
            await migrationScript(_database);

            // Record migration in history
            var record = new MigrationHistoryDocument
            {
                MigrationId = migrationId,
                AppliedAt = DateTimeHelper.UtcNow,
                ProductVersion = "1.0.0"
            };

            await history.InsertOneAsync(record);

            _logger.LogInformation("✅ MongoDB migration completed: {MigrationId}", migrationId);

            return new MongoMigrationResult(
                Success: true,
                MigrationId: migrationId,
                ExecutedAt: DateTimeHelper.UtcNow
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ MongoDB migration failed: {MigrationId}", migrationId);
            return new MongoMigrationResult(
                Success: false,
                MigrationId: migrationId,
                ExecutedAt: DateTimeHelper.UtcNow,
                ErrorMessage: ex.Message
            );
        }
    }

    /// <summary>
    /// Create an index on a MongoDB collection.
    /// Indexes improve query performance and enforce uniqueness constraints.
    /// </summary>
    public async Task CreateIndexAsync<T>(
        string collectionName,
        IndexKeysDefinition<T> keys,
        CreateIndexOptions? options = null)
    {
        var collection = _database.GetCollection<T>(collectionName);
        await collection.Indexes.CreateOneAsync(new CreateIndexModel<T>(keys, options));
        
        _logger.LogInformation("Created MongoDB index on collection: {CollectionName}", collectionName);
    }

    /// <summary>
    /// Ensure a collection exists with specific options.
    /// </summary>
    public async Task EnsureCollectionAsync(
        string collectionName,
        CreateCollectionOptions? options = null)
    {
        var collections = await _database.ListCollectionNamesAsync();
        var collectionNames = await collections.ToListAsync();

        if (!collectionNames.Contains(collectionName))
        {
            if (options != null)
            {
                await _database.CreateCollectionAsync(collectionName, options);
            }
            else
            {
                await _database.CreateCollectionAsync(collectionName);
            }

            _logger.LogInformation("Created MongoDB collection: {CollectionName}", collectionName);
        }
    }

    /// <summary>
    /// Get migration history from MongoDB.
    /// </summary>
    public async Task<IEnumerable<MigrationHistoryDocument>> GetMigrationHistoryAsync()
    {
        var history = _database.GetCollection<MigrationHistoryDocument>(_migrationCollectionName);
        var result = await history
            .Find(FilterDefinition<MigrationHistoryDocument>.Empty)
            .SortByDescending(m => m.AppliedAt)
            .ToListAsync();

        return result;
    }
}

/// <summary>
/// Extension methods for MongoDB migration setup in DI.
/// </summary>
public static class MongoMigrationExtensions
{
    /// <summary>
    /// Register MongoDB migration executor.
    /// </summary>
    public static IServiceCollection AddMongoMigrations(
        this IServiceCollection services,
        IMongoDatabase mongoDatabase)
    {
        services.AddScoped(sp =>
            new MongoMigrationExecutor(mongoDatabase, sp.GetRequiredService<ILogger<MongoMigrationExecutor>>()));

        return services;
    }

    /// <summary>
    /// Run a MongoDB migration.
    /// </summary>
    public static async Task RunMongoMigrationAsync(
        this IServiceProvider services,
        string migrationId,
        Func<IMongoDatabase, Task> migrationScript)
    {
        using var scope = services.CreateScope();
        var executor = scope.ServiceProvider.GetRequiredService<MongoMigrationExecutor>();
        var result = await executor.ExecuteAsync(migrationId, migrationScript);

        if (!result.Success)
        {
            throw new InvalidOperationException(
                $"MongoDB migration failed: {migrationId}. Error: {result.ErrorMessage}");
        }
    }
}
