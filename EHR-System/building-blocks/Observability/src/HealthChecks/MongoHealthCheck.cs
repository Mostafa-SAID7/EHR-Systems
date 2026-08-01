using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EHRPlatform.Observability.HealthChecks;

/// <summary>
/// Health check for MongoDB database.
/// Single responsibility: MongoDB connectivity check.
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
