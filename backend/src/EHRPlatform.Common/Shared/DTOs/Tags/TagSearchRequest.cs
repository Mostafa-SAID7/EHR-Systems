#nullable enable

using EHRPlatform.Common.Domain.Enums;

namespace EHRPlatform.Common.Shared.DTOs;

/// <summary>
/// Tag search request for advanced filtering.
/// </summary>
public record TagSearchRequest
{
    /// <summary>Resource type to search (e.g., "Patient", "Appointment").</summary>
    public required string ResourceType { get; init; }

    /// <summary>Tag IDs to filter by.</summary>
    public IEnumerable<Guid> TagIds { get; init; } = Enumerable.Empty<Guid>();

    /// <summary>Category filter for tags.</summary>
    public string? CategoryFilter { get; init; }

    /// <summary>Service name filter.</summary>
    public string? ServiceName { get; init; }

    /// <summary>Search mode (Any/All/Exact).</summary>
    public TagSearchMode Mode { get; init; } = TagSearchMode.Any;

    /// <summary>Pagination: page number (1-indexed).</summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>Pagination: page size.</summary>
    public int PageSize { get; init; } = 50;

    /// <summary>Date range filter (from).</summary>
    public DateTime? TaggedFrom { get; init; }

    /// <summary>Date range filter (to).</summary>
    public DateTime? TaggedTo { get; init; }

    /// <summary>Filter by who applied the tag.</summary>
    public string? AppliedBy { get; init; }
}
