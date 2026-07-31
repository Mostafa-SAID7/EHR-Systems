#nullable enable

using EHRPlatform.Common.Data.Migrations;
using EHRPlatform.Common.Domain.Enums;

namespace EHRPlatform.Common.Data.Migrations;

/// <summary>
/// Result of migration execution.
/// </summary>
public class MigrationResult
{
    /// <summary>
    /// Service name that ran the migration.
    /// </summary>
    public string ServiceName { get; set; } = string.Empty;

    /// <summary>
    /// Whether migration succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Migration strategy used.
    /// </summary>
    public MigrationPolicy Strategy { get; set; }

    /// <summary>
    /// Number of migrations applied.
    /// </summary>
    public int MigrationsApplied { get; set; }

    /// <summary>
    /// Number of migrations pending.
    /// </summary>
    public int MigrationsPending { get; set; }

    /// <summary>
    /// Error message if migration failed.
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// Human-readable summary.
    /// </summary>
    public override string ToString() =>
        $"{ServiceName}: {(Success ? "✅ Success" : "❌ Failed")} | Applied: {MigrationsApplied} | Pending: {MigrationsPending}";
}
