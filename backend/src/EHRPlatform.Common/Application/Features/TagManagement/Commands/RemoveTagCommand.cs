using EHRPlatform.Common.Application.Common.CQRS;
using EHRPlatform.Common.Application.Features.TagManagement.Responses;

namespace EHRPlatform.Common.Application.Features.TagManagement.Commands;

/// <summary>
/// Command to remove a tag from a resource.
/// Single responsibility: Command definition only.
/// </summary>
public record RemoveTagCommand : ICommand<TagAssignmentResponse>
{
    public required Guid ResourceId { get; init; }
    public required string ResourceType { get; init; }
    public required Guid TagId { get; init; }
    public required string ServiceName { get; init; }
}
