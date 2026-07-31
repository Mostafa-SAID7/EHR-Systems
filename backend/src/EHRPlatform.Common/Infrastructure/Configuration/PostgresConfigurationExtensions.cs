#nullable enable

using Microsoft.Extensions.Configuration;

namespace EHRPlatform.Common.Infrastructure.Configuration;

/// <summary>
/// Extension methods for PostgreSQL connection string configuration.
/// Single responsibility: Build PostgreSQL connection strings from environment and configuration.
/// </summary>
public static class PostgresConfigurationExtensions
{
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
}
