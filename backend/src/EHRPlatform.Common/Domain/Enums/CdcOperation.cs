#nullable enable

namespace EHRPlatform.Common.Domain.Enums;

/// <summary>
/// Change Data Capture (CDC) operation types.
/// Tracks what operation triggered a data change event.
/// </summary>
public enum CdcOperation
{
    /// <summary>Entity was created.</summary>
    Create = 0,

    /// <summary>Entity was updated.</summary>
    Update = 1,

    /// <summary>Entity was deleted.</summary>
    Delete = 2,

    /// <summary>Entity was restored (undeleted).</summary>
    Restore = 3,

    /// <summary>Batch operation on multiple entities.</summary>
    Batch = 4
}
