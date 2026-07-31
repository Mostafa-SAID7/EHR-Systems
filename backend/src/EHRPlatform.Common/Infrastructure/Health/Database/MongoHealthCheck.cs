using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;
using MongoDB.Driver;

namespace EHRPlatform.Common.Infrastructure.Health;

/// <summary>
/// Health check for MongoDB connectivity.
/// Runs a lightweight ping command against the configured database.
/// </summary>
public sealed class MongoHealthCheck : IHealthCheck
{
    private readonly IMongoDatabase _database;
    private readonly ILogger<MongoHealthCheck> _logger;

    public MongoHealthCheck(IMongoDatabase database, ILogger<MongoHealthCheck> logger)
    {
        _database = database ?? throw new ArgumentNullException(nameof(database));
        _logger   = logger   ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Ping is the cheapest operation that proves the connection is alive.
            await _database.RunCommandAsync(
                (Command<MongoDB.Bson.BsonDocument>)"{ping:1}",
                cancellationToken: cancellationToken);

            return HealthCheckResult.Healthy("MongoDB is reachable",
                new Dictionary<string, object> { { "database", _database.DatabaseNamespace.DatabaseName } });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MongoDB health check failed");
            return HealthCheckResult.Unhealthy("MongoDB is unreachable", ex);
        }
    }
}

/// <summary>Extension helpers for registering MongoDB health checks.</summary>
public static class MongoHealthCheckExtensions
{
    public static IHealthChecksBuilder AddMongoHealthCheck(
        this IHealthChecksBuilder builder,
        string? name = null,
        HealthStatus? failureStatus = null,
        IEnumerable<string>? tags = null,
        TimeSpan? timeout = null)
    {
        return builder.AddCheck<MongoHealthCheck>(
            name          ?? "mongodb",
            failureStatus ?? HealthStatus.Degraded,
            tags          ?? new[] { "db", "mongodb" },
            timeout       ?? TimeSpan.FromSeconds(5));
    }
}

