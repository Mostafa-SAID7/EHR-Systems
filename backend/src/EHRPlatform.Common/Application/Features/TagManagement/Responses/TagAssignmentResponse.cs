namespace EHRPlatform.Common.Application.Features.TagManagement.Responses;

/// <summary>
/// Response DTO for tag assignment operations.
/// Single responsibility: Response data transfer only.
/// </summary>
public class TagAssignmentResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid ResourceId { get; set; }
    public IEnumerable<Guid> AppliedTagIds { get; set; } = Enumerable.Empty<Guid>();
    public int TotalTagsOnResource { get; set; }
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();
}
