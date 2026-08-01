using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EHRPlatform.Observability.HealthChecks;

/// <summary>
/// Health check for database connectivity.
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly Func<CancellationToken, Task<bool>> _checkAsync;

    public DatabaseHealthCheck(Func<CancellationToken, Task<bool>> checkAsync)
    {
        _checkAsync = checkAsync ?? throw new ArgumentNullException(nameof(checkAsync));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var isHealthy = await _checkAsync(cancellationToken);
            return isHealthy
                ? HealthCheckResult.Healthy("Database connection successful")
                : HealthCheckResult.Unhealthy("Database query returned unexpected result");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Database health check failed: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// Health check for PostgreSQL database.
/// </summary>
public class PostgresHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public PostgresHealthCheck(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = new Npgsql.NpgsqlConnection(_connectionString);
            await connection.OpenAsync(cancellationToken);
            
            using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy("PostgreSQL connection successful");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"PostgreSQL health check failed: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// Health check for MongoDB database.
/// </summary>
public class MongoHealthCheck : IHealthCheck
{
    private readonly string _connectionString;
    private readonly string? _databaseName;

    public MongoHealthCheck(string connectionString, string? databaseName = null)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        _databaseName = databaseName;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var client = new MongoDB.Driver.MongoClient(_connectionString);
            var admin = client.GetDatabase("admin");
            
            await admin.RunCommandAsync(
                MongoDB.Bson.BsonDocument.Parse("{ ping: 1 }"),
                cancellationToken: cancellationToken);

            return HealthCheckResult.Healthy("MongoDB connection successful");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"MongoDB health check failed: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// Health check for Redis cache.
/// </summary>
public class RedisHealthCheck : IHealthCheck
{
    private readonly string _connectionString;

    public RedisHealthCheck(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            using var redis = StackExchange.Redis.ConnectionMultiplexer.Connect(_connectionString);
            var result = await redis.GetDatabase().StringGetAsync("health-check-key");
            return HealthCheckResult.Healthy("Redis connection successful");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy($"Redis health check failed: {ex.Message}", ex);
        }
    }
}

/// <summary>
/// Health check for RabbitMQ message broker.
/// </summary>
public class RabbitMqHealthCheck : IHealthCheck
{
    private readonly string _hostName;
    private readonly int _port;

    public RabbitMqHealthCheck(string hostName, int port = 5672)
    {
        _hostName = hostName ?? throw new ArgumentNullException(nameof(hostName));
        _port = port;
    }

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var factory = new RabbitMQ.Client.ConnectionFactory { HostName = _hostName, Port = _port };
            using var connection = factory.CreateConnection();
            connection.Close();
            
            return Task.FromResult(HealthCheckResult.Healthy("RabbitMQ connection successful"));
        }
        catch (Exception ex)
        {
            return Task.FromResult(HealthCheckResult.Unhealthy($"RabbitMQ health check failed: {ex.Message}", ex));
        }
    }
}
