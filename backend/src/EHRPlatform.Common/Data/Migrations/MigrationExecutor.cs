using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Common.Data.Migrations;

/// <summary>
/// Centralized migration executor for all services.
/// Handles automatic migrations on startup.
/// Strategy configurable per environment.
/// </summary>
public class MigrationExecutor
{
    private readonly ILogger<MigrationExecutor> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly MigrationPolicy _policy;

    public MigrationExecutor(
        ILogger<MigrationExecutor> logger,
        IServiceProvider serviceProvider,
        MigrationPolicy? policy = null)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _policy = policy ?? MigrationPolicy.AutomaticOnStartup;
    }

    /// <summary>
    /// Execute migrations for a specific DbContext type.
    /// </summary>
    public async Task<MigrationResult> ExecuteAsync<TContext>(
        string serviceName) where TContext : DbContext
    {
        var result = new MigrationResult { ServiceName = serviceName };

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<TContext>();

            _logger.LogInformation($"🔄 {serviceName}: Executing migration strategy: {_policy}");

            result = _policy switch
            {
                MigrationPolicy.AutomaticOnStartup => await ExecuteAutomatic(context, serviceName),
                MigrationPolicy.ManualOnly => await ExecuteManualCheck(context, serviceName),
                MigrationPolicy.Disabled => await ExecuteDisabled(context, serviceName),
                _ => throw new ArgumentException($"Unknown migration policy: {_policy}")
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ {serviceName}: Migration execution failed");
            result.Success = false;
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

    /// <summary>
    /// Execute all pending migrations automatically.
    /// Used in Development environments.
    /// </summary>
    private async Task<MigrationResult> ExecuteAutomatic<TContext>(
        TContext context,
        string serviceName) where TContext : DbContext
    {
        try
        {
            var canConnect = await context.CanConnectAsync(_logger, serviceName);
            if (!canConnect)
            {
                throw new InvalidOperationException($"{serviceName}: Cannot connect to database");
            }

            var success = await context.MigrateAsync(_logger, serviceName);
            
            var info = await context.GetMigrationInfoAsync();
            await context.LogMigrationStatusAsync(_logger, serviceName);

            return new MigrationResult
            {
                ServiceName = serviceName,
                Success = success,
                Strategy = MigrationPolicy.AutomaticOnStartup,
                MigrationsApplied = info.AppliedCount,
                MigrationsPending = info.PendingCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ {serviceName}: Automatic migration failed");
            throw;
        }
    }

    /// <summary>
    /// Check if migrations are needed, but don't apply them.
    /// Used in Staging/Production - migrations applied separately.
    /// </summary>
    private async Task<MigrationResult> ExecuteManualCheck<TContext>(
        TContext context,
        string serviceName) where TContext : DbContext
    {
        try
        {
            var pending = (await context.GetPendingMigrationsAsync()).ToList();
            var applied = (await context.GetAppliedMigrationsAsync()).ToList();

            if (pending.Any())
            {
                _logger.LogWarning($"⚠️ {serviceName}: {pending.Count} pending migration(s) - manual intervention required");
                _logger.LogWarning($"   Pending: {string.Join(", ", pending)}");
            }
            else
            {
                _logger.LogInformation($"✅ {serviceName}: All migrations applied (manual mode)");
            }

            return new MigrationResult
            {
                ServiceName = serviceName,
                Success = true,
                Strategy = MigrationPolicy.ManualOnly,
                MigrationsApplied = applied.Count,
                MigrationsPending = pending.Count
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ {serviceName}: Migration check failed");
            throw;
        }
    }

    /// <summary>
    /// Disable automatic migrations - assume database is already migrated.
    /// Used in highly controlled production environments.
    /// </summary>
    private async Task<MigrationResult> ExecuteDisabled<TContext>(
        TContext context,
        string serviceName) where TContext : DbContext
    {
        try
        {
            var canConnect = await context.CanConnectAsync(_logger, serviceName);

            if (!canConnect)
            {
                throw new InvalidOperationException($"{serviceName}: Cannot connect to database - cannot verify schema");
            }

            _logger.LogInformation($"ℹ️ {serviceName}: Automatic migrations disabled - assuming database is pre-migrated");

            var info = await context.GetMigrationInfoAsync();

            return new MigrationResult
            {
                ServiceName = serviceName,
                Success = true,
                Strategy = MigrationPolicy.Disabled,
                MigrationsApplied = info.AppliedCount,
                MigrationsPending = info.PendingCount
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"❌ {serviceName}: Database connection verification failed");
            throw;
        }
    }
}

/// <summary>
/// Migration policy - determines when/how migrations are executed.
/// </summary>
public enum MigrationPolicy
{
    /// <summary>
    /// Automatically apply all pending migrations on service startup.
    /// Suitable for Development.
    /// </summary>
    AutomaticOnStartup,

    /// <summary>
    /// Check for pending migrations but don't apply them.
    /// Migrations must be applied manually via scripts.
    /// Suitable for Staging/Production.
    /// </summary>
    ManualOnly,

    /// <summary>
    /// Skip all migration checks and execution.
    /// Database must be pre-migrated before deployment.
    /// Suitable for highly controlled production environments.
    /// </summary>
    Disabled
}

/// <summary>
/// Result of migration execution.
/// </summary>
public class MigrationResult
{
    public string ServiceName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public MigrationPolicy Strategy { get; set; }
    public int MigrationsApplied { get; set; }
    public int MigrationsPending { get; set; }
    public string? ErrorMessage { get; set; }

    public override string ToString() =>
        $"{ServiceName}: {(Success ? "✅ Success" : "❌ Failed")} | Applied: {MigrationsApplied} | Pending: {MigrationsPending}";
}
