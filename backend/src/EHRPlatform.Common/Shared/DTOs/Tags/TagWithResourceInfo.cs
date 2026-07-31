#nullable enable

namespace EHRPlatform.Common.Shared.DTOs;

/// <summary>
/// Tag with associated resource info.
/// </summary>
public record TagWithResourceInfo
{
    /// <summary>Tag ID.</summary>
    public required Guid TagId { get; init; }

    /// <summary>Tag name.</summary>
    public required string TagName { get; init; }

    /// <summary>Tag slug.</summary>
    public required string TagSlug { get; init; }

    /// <summary>Resource ID tagged with this tag.</summary>
    public required Guid ResourceId { get; init; }

    /// <summary>Resource type.</summary>
    public required string ResourceType { get; init; }

    /// <summary>Service name.</summary>
    public required string ServiceName { get; init; }

    /// <summary>When tag was applied.</summary>
    public required DateTime AppliedAt { get; init; }

    /// <summary>Who applied the tag.</summary>
    public string? AppliedBy { get; init; }
}
