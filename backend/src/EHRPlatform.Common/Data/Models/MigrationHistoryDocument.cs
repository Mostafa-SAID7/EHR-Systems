#nullable enable

namespace EHRPlatform.Common.Data.Migrations;

/// <summary>
/// Migration history document for MongoDB tracking.
/// Single responsibility: Track applied migrations only.
/// </summary>
public class MigrationHistoryDocument
{
    /// <summary>
    /// Migration ID identifier.
    /// </summary>
    public string MigrationId { get; set; } = string.Empty;

    /// <summary>
    /// When this migration was applied.
    /// </summary>
    public DateTime AppliedAt { get; set; }

    /// <summary>
    /// Product version when migration was applied.
    /// </summary>
    public string ProductVersion { get; set; } = "1.0.0";
}
