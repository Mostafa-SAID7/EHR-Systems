using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Common.Data.Migrations;

/// <summary>
/// Extension methods for database migrations.
/// Provides safe migration execution with logging and error handling.
/// </summary>
public static class MigrationExtensions
{
    /// <summary>
    /// Apply all pending migrations with logging and error handling.
    /// Safe to call multiple times (idempotent).
    /// </summary>
    public static async Task<bool> MigrateAsync<TContext>(
        this TContext context,
        ILogger logger,
        string serviceName = "Service") where TContext : DbContext
    {
        try
        {
            var pendingMigrations = (await context.Database.GetPendingMigrationsAsync()).ToList();
            
            if (!pendingMigrations.Any())
            {
                logger.LogInformation($"✅ {serviceName}: No pending migrations");
                return true;
            }

            logger.LogInformation($"⏳ {serviceName}: Applying {pendingMigrations.Count} pending migration(s): {string.Join(", ", pendingMigrations)}");
            
            await context.Database.MigrateAsync();
            
            logger.LogInformation($"✅ {serviceName}: All migrations applied successfully");
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"❌ {serviceName}: Migration failed");
            return false;
        }
    }

    /// <summary>
    /// Get all applied migrations.
    /// </summary>
    public static async Task<IEnumerable<string>> GetAppliedMigrationsAsync<TContext>(
        this TContext context) where TContext : DbContext
    {
        return await context.Database.GetAppliedMigrationsAsync();
    }

    /// <summary>
    /// Get all pending migrations.
    /// </summary>
    public static async Task<IEnumerable<string>> GetPendingMigrationsAsync<TContext>(
        this TContext context) where TContext : DbContext
    {
        return await context.Database.GetPendingMigrationsAsync();
    }

    /// <summary>
    /// Check if database exists and is accessible.
    /// </summary>
    public static async Task<bool> CanConnectAsync<TContext>(
        this TContext context,
        ILogger logger,
        string serviceName = "Service") where TContext : DbContext
    {
        try
        {
            var canConnect = await context.Database.CanConnectAsync();
            if (canConnect)
            {
                logger.LogInformation($"✅ {serviceName}: Database connection successful");
            }
            else
            {
                logger.LogWarning($"⚠️ {serviceName}: Cannot connect to database");
            }
            return canConnect;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"❌ {serviceName}: Database connection check failed");
            return false;
        }
    }

    /// <summary>
    /// Ensure database exists (create if not exists).
    /// </summary>
    public static async Task<bool> EnsureDatabaseExistsAsync<TContext>(
        this TContext context,
        ILogger logger,
        string serviceName = "Service") where TContext : DbContext
    {
        try
        {
            var created = await context.Database.EnsureCreatedAsync();
            if (created)
            {
                logger.LogInformation($"✅ {serviceName}: Database created");
            }
            else
            {
                logger.LogInformation($"ℹ️ {serviceName}: Database already exists");
            }
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"❌ {serviceName}: Failed to ensure database exists");
            return false;
        }
    }

    /// <summary>
    /// Get migration info (applied migrations + pending).
    /// </summary>
    public static async Task<MigrationInfo> GetMigrationInfoAsync<TContext>(
        this TContext context) where TContext : DbContext
    {
        var applied = (await context.Database.GetAppliedMigrationsAsync()).ToList();
        var pending = (await context.Database.GetPendingMigrationsAsync()).ToList();

        return new MigrationInfo
        {
            AppliedCount = applied.Count,
            PendingCount = pending.Count,
            AppliedMigrations = applied,
            PendingMigrations = pending,
            IsUpToDate = !pending.Any()
        };
    }

    /// <summary>
    /// Log migration status report.
    /// </summary>
    public static async Task LogMigrationStatusAsync<TContext>(
        this TContext context,
        ILogger logger,
        string serviceName = "Service") where TContext : DbContext
    {
        var info = await context.GetMigrationInfoAsync();
        
        logger.LogInformation($"📊 {serviceName} Migration Status:");
        logger.LogInformation($"   Applied: {info.AppliedCount}");
        logger.LogInformation($"   Pending: {info.PendingCount}");
        logger.LogInformation($"   Up-to-date: {(info.IsUpToDate ? "✅ Yes" : "❌ No")}");
        
        if (info.AppliedMigrations.Any())
        {
            logger.LogDebug($"   Applied migrations: {string.Join(", ", info.AppliedMigrations)}");
        }
        
        if (info.PendingMigrations.Any())
        {
            logger.LogWarning($"   Pending migrations: {string.Join(", ", info.PendingMigrations)}");
        }
    }
}

/// <summary>
/// Migration information snapshot.
/// </summary>
public class MigrationInfo
{
    public int AppliedCount { get; set; }
    public int PendingCount { get; set; }
    public List<string> AppliedMigrations { get; set; } = new();
    public List<string> PendingMigrations { get; set; } = new();
    public bool IsUpToDate { get; set; }
}
