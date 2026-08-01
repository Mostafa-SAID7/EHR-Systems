#nullable enable

using Microsoft.Extensions.Configuration;

namespace EHRPlatform.Common.Infrastructure.Configuration;

/// <summary>
/// Extension methods for MySQL connection string configuration.
/// Single responsibility: Build MySQL connection strings from environment and configuration.
/// </summary>
public static class MySqlConfigurationExtensions
{
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
}
