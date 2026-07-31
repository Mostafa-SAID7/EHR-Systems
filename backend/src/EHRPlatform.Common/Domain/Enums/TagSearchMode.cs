#nullable enable

namespace EHRPlatform.Common.Domain.Enums;

/// <summary>
/// Tag search mode for multi-tag filtering queries.
/// Determines how multiple tag IDs are combined in search logic.
/// </summary>
public enum TagSearchMode
{
    /// <summary>Resource must have ANY of the specified tags (OR logic).</summary>
    Any = 0,

    /// <summary>Resource must have ALL of the specified tags (AND logic).</summary>
    All = 1,

    /// <summary>Resource must have EXACTLY these tags (exact match).</summary>
    Exact = 2
}
