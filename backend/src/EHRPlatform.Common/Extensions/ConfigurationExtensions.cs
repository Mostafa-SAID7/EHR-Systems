using Microsoft.Extensions.Configuration;

namespace EHRPlatform.Common.Extensions;

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

    // ─── MySQL ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Build a MySQL connection string from environment variables or configuration.
    ///
    /// Environment variable priority:
    ///   1. MYSQL_CONNECTION_STRING — full DSN (wins immediately when set).
    ///   2. MYSQL_HOST / MYSQL_PORT / MYSQL_DATABASE / MYSQL_USER / MYSQL_PASSWORD
    ///      individual variables (mirrors common Docker/Kubernetes patterns).
    ///   3. ConnectionStrings:MySqlConnection in appsettings.json.
    ///   4. Returns <c>null</c> when none is configured.
    /// </summary>
    public static string? BuildMySqlConnectionString(this IConfiguration config)
    {
        // Full DSN takes highest priority
        var fullDsn = Environment.GetEnvironmentVariable("MYSQL_CONNECTION_STRING")
            ?? config.GetConnectionString("MySqlConnection");
        if (!string.IsNullOrEmpty(fullDsn)) return fullDsn;

        // Build from parts
        var host = Environment.GetEnvironmentVariable("MYSQL_HOST") ?? config["MySQL:Host"];
        if (string.IsNullOrEmpty(host)) return null;

        var port   = Environment.GetEnvironmentVariable("MYSQL_PORT")     ?? config["MySQL:Port"]     ?? "3306";
        var db     = Environment.GetEnvironmentVariable("MYSQL_DATABASE") ?? config["MySQL:Database"] ?? string.Empty;
        var user   = Environment.GetEnvironmentVariable("MYSQL_USER")     ?? config["MySQL:Username"] ?? "root";
        var pass   = Environment.GetEnvironmentVariable("MYSQL_PASSWORD") ?? config["MySQL:Password"] ?? string.Empty;

        return $"Server={host};Port={port};Database={db};Uid={user};Pwd={pass};CharSet=utf8mb4;AllowPublicKeyRetrieval=true;SslMode=None;";
    }
}
