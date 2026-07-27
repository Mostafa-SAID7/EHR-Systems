using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;

namespace EHRPlatform.Common.Data.Migrations;

/// <summary>
/// MySQL migration executor for relational data.
/// Handles schema versioning, index creation, and data transformation for MySQL.
/// Supports MySQL 5.7+ and MySQL 8.0+ with full InnoDB support.
/// </summary>
public class MySqlMigrationExecutor
{
    private readonly ILogger<MySqlMigrationExecutor> _logger;
    private readonly string _connectionString;
    private readonly string _migrationTableName = "__MigrationHistory";

    public MySqlMigrationExecutor(string connectionString, ILogger<MySqlMigrationExecutor> logger)
    {
        _connectionString = connectionString;
        _logger = logger;
    }

    /// <summary>
    /// Result of a MySQL migration execution.
    /// </summary>
    public record MySqlMigrationResult(
        bool Success,
        string MigrationId,
        DateTime ExecutedAt,
        string? ErrorMessage = null,
        int RowsAffected = 0,
        long ExecutionTimeMs = 0
    );

    /// <summary>
    /// Execute a MySQL migration script.
    /// Migrations are idempotent — executing twice is safe.
    /// Includes automatic rollback on failure (if in transaction).
    /// </summary>
    public async Task<MySqlMigrationResult> ExecuteAsync(
        string migrationId,
        Func<MySqlConnection, MySqlTransaction, Task<int>> migrationScript)
    {
        var startTime = DateTime.UtcNow;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        using var connection = new MySqlConnection(_connectionString);

        try
        {
            await connection.OpenAsync();
            _logger.LogInformation("Starting MySQL migration: {MigrationId}", migrationId);

            // Ensure migration history table exists
            await EnsureMigrationTableAsync(connection);

            // Check if migration already applied
            if (await IsMigrationAppliedAsync(connection, migrationId))
            {
                _logger.LogWarning("MySQL migration already applied: {MigrationId}", migrationId);
                stopwatch.Stop();
                return new MySqlMigrationResult(
                    Success: true,
                    MigrationId: migrationId,
                    ExecutedAt: startTime,
                    ErrorMessage: "Already applied",
                    ExecutionTimeMs: stopwatch.ElapsedMilliseconds
                );
            }

            // Start transaction
            using var transaction = await connection.BeginTransactionAsync();

            try
            {
                // Execute migration script
                var rowsAffected = await migrationScript(connection, transaction);

                // Record migration in history
                await RecordMigrationAsync(connection, transaction, migrationId);

                // Commit transaction
                await transaction.CommitAsync();

                stopwatch.Stop();
                _logger.LogInformation(
                    "✅ MySQL migration completed: {MigrationId} ({ExecutionTime}ms, {RowsAffected} rows)",
                    migrationId, stopwatch.ElapsedMilliseconds, rowsAffected);

                return new MySqlMigrationResult(
                    Success: true,
                    MigrationId: migrationId,
                    ExecutedAt: startTime,
                    RowsAffected: rowsAffected,
                    ExecutionTimeMs: stopwatch.ElapsedMilliseconds
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                stopwatch.Stop();
                _logger.LogError(ex, "❌ MySQL migration failed (rolled back): {MigrationId}", migrationId);
                throw;
            }
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            _logger.LogError(ex, "❌ MySQL migration failed: {MigrationId}", migrationId);
            return new MySqlMigrationResult(
                Success: false,
                MigrationId: migrationId,
                ExecutedAt: startTime,
                ErrorMessage: ex.Message,
                ExecutionTimeMs: stopwatch.ElapsedMilliseconds
            );
        }
        finally
        {
            await connection.CloseAsync();
        }
    }

    /// <summary>
    /// Execute raw SQL command (for schema changes, DDL).
    /// </summary>
    public async Task<int> ExecuteCommandAsync(
        string sql,
        MySqlConnection connection,
        MySqlTransaction transaction)
    {
        using var command = new MySqlCommand(sql, connection, transaction);
        return await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Execute query and return results.
    /// </summary>
    public async Task<T?> ExecuteScalarAsync<T>(
        string sql,
        MySqlConnection connection,
        MySqlTransaction? transaction = null)
    {
        using var command = new MySqlCommand(sql, connection, transaction);
        var result = await command.ExecuteScalarAsync();
        return result != null ? (T)Convert.ChangeType(result, typeof(T)) : default;
    }

    /// <summary>
    /// Ensure migration history table exists.
    /// </summary>
    private async Task EnsureMigrationTableAsync(MySqlConnection connection)
    {
        var createTableSql = $@"
            CREATE TABLE IF NOT EXISTS `{_migrationTableName}` (
                `id` INT AUTO_INCREMENT PRIMARY KEY,
                `MigrationId` VARCHAR(255) NOT NULL UNIQUE,
                `ProductVersion` VARCHAR(50) NOT NULL,
                `AppliedAt` TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
                INDEX `idx_migrationid` (`MigrationId`),
                INDEX `idx_appliedat` (`AppliedAt`)
            ) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci";

        using var command = new MySqlCommand(createTableSql, connection);
        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Check if migration was already applied.
    /// </summary>
    private async Task<bool> IsMigrationAppliedAsync(MySqlConnection connection, string migrationId)
    {
        var query = $@"
            SELECT COUNT(*) FROM `{_migrationTableName}` 
            WHERE `MigrationId` = @MigrationId";

        using var command = new MySqlCommand(query, connection);
        command.Parameters.AddWithValue("@MigrationId", migrationId);

        var result = await command.ExecuteScalarAsync();
        return (long)(result ?? 0) > 0;
    }

    /// <summary>
    /// Record migration in history table.
    /// </summary>
    private async Task RecordMigrationAsync(
        MySqlConnection connection,
        MySqlTransaction transaction,
        string migrationId)
    {
        var insertSql = $@"
            INSERT INTO `{_migrationTableName}` (`MigrationId`, `ProductVersion`)
            VALUES (@MigrationId, @ProductVersion)";

        using var command = new MySqlCommand(insertSql, connection, transaction);
        command.Parameters.AddWithValue("@MigrationId", migrationId);
        command.Parameters.AddWithValue("@ProductVersion", "1.0.0");

        await command.ExecuteNonQueryAsync();
    }

    /// <summary>
    /// Get migration history.
    /// </summary>
    public async Task<IEnumerable<MySqlMigrationHistoryRecord>> GetMigrationHistoryAsync()
    {
        var results = new List<MySqlMigrationHistoryRecord>();

        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = $@"
            SELECT `MigrationId`, `ProductVersion`, `AppliedAt` 
            FROM `{_migrationTableName}`
            ORDER BY `AppliedAt` DESC";

        using var command = new MySqlCommand(query, connection);
        using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            results.Add(new MySqlMigrationHistoryRecord(
                MigrationId: reader.GetString(0),
                ProductVersion: reader.GetString(1),
                AppliedAt: reader.GetDateTime(2)
            ));
        }

        return results;
    }

    /// <summary>
    /// Verify database health and connectivity.
    /// </summary>
    public async Task<bool> HealthCheckAsync()
    {
        try
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            
            using var command = new MySqlCommand("SELECT 1", connection);
            await command.ExecuteScalarAsync();
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "MySQL health check failed");
            return false;
        }
    }
}

/// <summary>
/// MySQL migration history record.
/// </summary>
public record MySqlMigrationHistoryRecord(
    string MigrationId,
    string ProductVersion,
    DateTime AppliedAt
);

/// <summary>
/// Extension methods for MySQL migration setup in DI.
/// </summary>
public static class MySqlMigrationExtensions
{
    /// <summary>
    /// Register MySQL migration executor.
    /// </summary>
    public static IServiceCollection AddMySqlMigrations(
        this IServiceCollection services,
        string connectionString)
    {
        services.AddScoped(sp =>
            new MySqlMigrationExecutor(
                connectionString,
                sp.GetRequiredService<ILogger<MySqlMigrationExecutor>>()
            ));

        return services;
    }

    /// <summary>
    /// Run a MySQL migration.
    /// </summary>
    public static async Task RunMySqlMigrationAsync(
        this IServiceProvider services,
        string migrationId,
        Func<MySqlConnection, MySqlTransaction, Task<int>> migrationScript)
    {
        using var scope = services.CreateScope();
        var executor = scope.ServiceProvider.GetRequiredService<MySqlMigrationExecutor>();
        var result = await executor.ExecuteAsync(migrationId, migrationScript);

        if (!result.Success)
        {
            throw new InvalidOperationException(
                $"MySQL migration failed: {migrationId}. Error: {result.ErrorMessage}");
        }
    }
}
