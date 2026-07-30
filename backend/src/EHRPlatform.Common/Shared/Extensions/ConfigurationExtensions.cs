using Microsoft.Extensions.Configuration;

namespace EHRPlatform.Common.Shared.Extensions;

/// <summary>
/// Extension methods for <see cref="IConfiguration"/> used across all EHR microservices.
/// Provides helpers for building database connection strings from Replit environment
/// variables (PGHOST, MONGODB_URI, MYSQL_HOST …) with clean fallback to explicit
/// configuration values.
/// </summary>
public static class ConfigurationExtensions
{
    // ─── PostgreSQL ──────────────────────────────────────────────────────────

    /// <summary>
    /// Build a Npgsql connection string from Replit PG* environment variables,
    /// falling back gracefully to an explicit connection string in configuration.
    ///
    /// Priority:
    ///   1. PG* env vars (always wins when PGHOST is set, even if DefaultConnection
    ///      exists but points to localhost — that is the docker-compose default, not
    ///      the live Replit DB).
    ///   2. Explicit ConnectionStrings:DefaultConnection that does NOT contain "localhost".
    ///   3. Throw — no database configured at all.
    /// </summary>
    public static string BuildPostgresConnectionString(this IConfiguration config)
    {
        var explicit_ = config.GetConnectionString("DefaultConnection");

        var host = Environment.GetEnvironmentVariable("PGHOST");
        var port = Environment.GetEnvironmentVariable("PGPORT") ?? "5432";
        var db   = Environment.GetEnvironmentVariable("PGDATABASE");
        var user = Environment.GetEnvironmentVariable("PGUSER");
        var pass = Environment.GetEnvironmentVariable("PGPASSWORD");

        if (!string.IsNullOrEmpty(host))
        {
            // Prefer env vars when PGHOST is present, unless an explicit non-localhost
            // connection string is configured (e.g. external cloud database).
            if (string.IsNullOrEmpty(explicit_) || explicit_.Contains("localhost"))
            {
                var needsSsl = host.Contains('.');
                var sslClause = needsSsl
                    ? "SSL Mode=Require;Trust Server Certificate=true;"
                    : "SSL Mode=Disable;";
                return $"Host={host};Port={port};Database={db};Username={user};Password={pass};{sslClause}";
            }
        }

        if (!string.IsNullOrEmpty(explicit_)) return explicit_;

        throw new InvalidOperationException(
            "PostgreSQL connection not configured. Set PGHOST (Replit managed PostgreSQL) " +
            "or ConnectionStrings__DefaultConnection.");
    }

    // ─── MySQL ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Build a MySQL connection string from MYSQL_* environment variables,
    /// falling back to an explicit connection string in configuration.
    ///
    /// Priority:
    ///   1. MYSQL_* env vars (wins when MYSQL_HOST is set).
    ///   2. Explicit ConnectionStrings:MySqlConnection that does NOT contain "localhost".
    ///   3. Returns null — callers should decide whether to throw.
    /// </summary>
    public static string? BuildMysqlConnectionString(this IConfiguration config)
    {
        var host = Environment.GetEnvironmentVariable("MYSQL_HOST")
                ?? Environment.GetEnvironmentVariable("MYSQLHOST");
        var port = Environment.GetEnvironmentVariable("MYSQL_PORT")
                ?? Environment.GetEnvironmentVariable("MYSQLPORT") ?? "3306";
        var db   = Environment.GetEnvironmentVariable("MYSQL_DATABASE")
                ?? Environment.GetEnvironmentVariable("MYSQLDATABASE");
        var user = Environment.GetEnvironmentVariable("MYSQL_USER")
                ?? Environment.GetEnvironmentVariable("MYSQLUSER");
        var pass = Environment.GetEnvironmentVariable("MYSQL_PASSWORD")
                ?? Environment.GetEnvironmentVariable("MYSQLPASSWORD");

        if (!string.IsNullOrEmpty(host) && !string.IsNullOrEmpty(db))
        {
            return $"Server={host};Port={port};Database={db};User={user};Password={pass};AllowPublicKeyRetrieval=true;SslMode=preferred;";
        }

        var explicit_ = config.GetConnectionString("MySqlConnection");
        if (!string.IsNullOrEmpty(explicit_)) return explicit_;

        return null;
    }

    // ─── MongoDB ─────────────────────────────────────────────────────────────

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

