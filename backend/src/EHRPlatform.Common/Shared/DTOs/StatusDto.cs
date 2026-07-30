#nullable enable

namespace EHRPlatform.Common.Shared.DTOs;

/// <summary>
/// Base response DTO with status tracking and slug support.
/// Use for entities that have a Status field.
/// </summary>
public abstract class StatusDto : SluggedResponseDto
{
    /// <summary>
    /// Current status as string (e.g., "Active", "Draft", "Completed").
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// URL-friendly slug representation of the status.
    /// </summary>
    public string? StatusSlug { get; set; }
}
