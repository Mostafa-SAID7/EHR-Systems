#nullable enable

namespace EHRPlatform.Common.Domain.Enums;

/// <summary>
/// Migration policy - determines when/how database migrations are executed.
/// </summary>
public enum MigrationPolicy
{
    /// <summary>
    /// Automatically apply all pending migrations on service startup.
    /// Suitable for Development environments.
    /// </summary>
    AutomaticOnStartup = 0,

    /// <summary>
    /// Check for pending migrations but don't apply them.
    /// Migrations must be applied manually via scripts.
    /// Suitable for Staging/Production environments.
    /// </summary>
    ManualOnly = 1,

    /// <summary>
    /// Skip all migration checks and execution.
    /// Database must be pre-migrated before deployment.
    /// Suitable for highly controlled production environments.
    /// </summary>
    Disabled = 2
}
