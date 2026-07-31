using EHRPlatform.Common.Application.Common.CQRS;
using EHRPlatform.Common.Application.Features.TagManagement.Responses;

namespace EHRPlatform.Common.Application.Features.TagManagement.Commands;

/// <summary>
/// Command to apply tags to a resource.
/// Single responsibility: Command definition only.
/// </summary>
public record ApplyTagsCommand : ICommand<TagAssignmentResponse>
{
    public required Guid ResourceId { get; init; }
    public required string ResourceType { get; init; }
    public required IEnumerable<Guid> TagIds { get; init; }
    public required string ServiceName { get; init; }
    public string? Context { get; init; }
    public string? AppliedBy { get; init; }
}
