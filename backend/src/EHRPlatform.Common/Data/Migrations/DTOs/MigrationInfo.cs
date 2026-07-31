#nullable enable

namespace EHRPlatform.Common.Data.Migrations;

/// <summary>
/// Migration information snapshot.
/// Single responsibility: Hold migration state data only.
/// </summary>
public class MigrationInfo
{
    /// <summary>
    /// Number of migrations already applied to database.
    /// </summary>
    public int AppliedCount { get; set; }

    /// <summary>
    /// Number of migrations pending application.
    /// </summary>
    public int PendingCount { get; set; }

    /// <summary>
    /// List of applied migration IDs.
    /// </summary>
    public List<string> AppliedMigrations { get; set; } = new();

    /// <summary>
    /// List of pending migration IDs.
    /// </summary>
    public List<string> PendingMigrations { get; set; } = new();

    /// <summary>
    /// Whether database schema is fully up-to-date.
    /// </summary>
    public bool IsUpToDate { get; set; }
}
