#nullable enable

using Microsoft.Extensions.Configuration;

namespace EHRPlatform.Common.Infrastructure.Configuration;

/// <summary>
/// Extension methods for MongoDB connection string and database configuration.
/// Single responsibility: Build MongoDB connection strings and resolve database names from environment and configuration.
/// </summary>
public static class MongoConfigurationExtensions
{
    /// <summary>
    /// Resolve a MongoDB connection string from the following priority order:
    ///   1. MONGODB_URI environment variable (full URI, e.g. Atlas or Replit add-on).
    ///   2. MONGODB_CONNECTION_STRING environment variable.
    ///   3. MongoDB:ConnectionString in appsettings.json.
    ///   4. Returns <c>null</c> when none is configured — callers should degrade
    ///      gracefully rather than throwing.
    /// </summary>
    public static string? BuildMongoConnectionString(this IConfiguration config)
    {
        return Environment.GetEnvironmentVariable("MONGODB_URI")
            ?? Environment.GetEnvironmentVariable("MONGODB_CONNECTION_STRING")
            ?? config["MongoDB:ConnectionString"];
    }

    /// <summary>
    /// Resolve a MongoDB database name from:
    ///   1. MONGODB_DATABASE environment variable.
    ///   2. MongoDB:DatabaseName in appsettings.json.
    ///   3. <paramref name="fallbackName"/> (service-specific default).
    /// </summary>
    public static string BuildMongoDatabaseName(
        this IConfiguration config,
        string fallbackName)
    {
        return Environment.GetEnvironmentVariable("MONGODB_DATABASE")
            ?? config["MongoDB:DatabaseName"]
            ?? fallbackName;
    }
}
