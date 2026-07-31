#nullable enable

namespace EHRPlatform.Common.Shared.DTOs;

/// <summary>
/// Tag usage statistics.
/// </summary>
public record TagUsageStatistic
{
    /// <summary>Tag ID.</summary>
    public required Guid TagId { get; init; }

    /// <summary>Tag name.</summary>
    public required string TagName { get; init; }

    /// <summary>Tag category.</summary>
    public required string Category { get; init; }

    /// <summary>Number of resources with this tag.</summary>
    public required int UsageCount { get; init; }

    /// <summary>Last applied date.</summary>
    public required DateTime? LastAppliedAt { get; init; }

    /// <summary>Percentage of total tags (0-100).</summary>
    public double UsagePercentage { get; init; }
}
