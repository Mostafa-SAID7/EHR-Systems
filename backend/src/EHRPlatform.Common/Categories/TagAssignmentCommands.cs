#nullable enable

using Microsoft.Extensions.Logging;
using EHRPlatform.Common.Application.CQRS;

namespace EHRPlatform.Common.Tags;

/// <summary>
/// Command to apply tags to a resource.
/// </summary>
public record ApplyTagsCommand : ICommand<TagAssignmentResponse>
{
    /// <summary>
    /// Resource ID.
    /// </summary>
    public required Guid ResourceId { get; init; }

    /// <summary>
    /// Resource type (e.g., "Patient", "Appointment").
    /// </summary>
    public required string ResourceType { get; init; }

    /// <summary>
    /// Tag IDs to apply.
    /// </summary>
    public required IEnumerable<Guid> TagIds { get; init; }

    /// <summary>
    /// Service name.
    /// </summary>
    public required string ServiceName { get; init; }

    /// <summary>
    /// Optional context about tag application.
    /// </summary>
    public string? Context { get; init; }

    /// <summary>
    /// User ID applying the tags.
    /// </summary>
    public string? AppliedBy { get; init; }
}

/// <summary>
/// Command to remove a tag from a resource.
/// </summary>
public record RemoveTagCommand : ICommand<TagAssignmentResponse>
{
    /// <summary>
    /// Resource ID.
    /// </summary>
    public required Guid ResourceId { get; init; }

    /// <summary>
    /// Resource type.
    /// </summary>
    public required string ResourceType { get; init; }

    /// <summary>
    /// Tag ID to remove.
    /// </summary>
    public required Guid TagId { get; init; }

    /// <summary>
    /// Service name.
    /// </summary>
    public required string ServiceName { get; init; }
}

/// <summary>
/// Command to set tags on a resource (replaces all existing).
/// </summary>
public record SetResourceTagsCommand : ICommand<TagAssignmentResponse>
{
    /// <summary>
    /// Resource ID.
    /// </summary>
    public required Guid ResourceId { get; init; }

    /// <summary>
    /// Resource type.
    /// </summary>
    public required string ResourceType { get; init; }

    /// <summary>
    /// Tag IDs to set.
    /// </summary>
    public required IEnumerable<Guid> TagIds { get; init; }

    /// <summary>
    /// Service name.
    /// </summary>
    public required string ServiceName { get; init; }

    /// <summary>
    /// User ID applying the tags.
    /// </summary>
    public string? AppliedBy { get; init; }
}

/// <summary>
/// Command handler for ApplyTagsCommand.
/// </summary>
public class ApplyTagsCommandHandler : ICommandHandler<ApplyTagsCommand, TagAssignmentResponse>
{
    private readonly ITagService _tagService;
    private readonly ILogger<ApplyTagsCommandHandler> _logger;

    public ApplyTagsCommandHandler(ITagService tagService, ILogger<ApplyTagsCommandHandler> logger)
    {
        _tagService = tagService;
        _logger = logger;
    }

    public async Task<TagAssignmentResponse> Handle(
        ApplyTagsCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var errors = new List<string>();
            var appliedIds = new List<Guid>();
            var tagIdList = command.TagIds.ToList();

            // Validate all tags exist and can be used by service
            foreach (var tagId in tagIdList)
            {
                var tag = await _tagService.GetByIdAsync(tagId, cancellationToken);
                
                if (tag == null)
                {
                    errors.Add($"Tag {tagId} not found");
                    continue;
                }

                if (!tag.CanBeUsedByService(command.ServiceName))
                {
                    errors.Add($"Tag '{tag.Name}' cannot be used by service {command.ServiceName}");
                    continue;
                }

                // Apply tag
                await _tagService.ApplyTagAsync(
                    command.ResourceId,
                    command.ResourceType,
                    tagId,
                    command.ServiceName,
                    command.Context,
                    command.AppliedBy,
                    cancellationToken);

                appliedIds.Add(tagId);
            }

            // Get updated tag count
            var allTags = await _tagService.GetResourceTagsAsync(
                command.ResourceId,
                command.ResourceType,
                cancellationToken);

            var success = errors.Count == 0;
            var message = success
                ? $"Successfully applied {appliedIds.Count} tag(s)"
                : $"Partially applied: {appliedIds.Count}/{tagIdList.Count} tags. Errors: {string.Join("; ", errors)}";

            _logger.LogInformation(
                "Applied {Count} tags to {ResourceType} {ResourceId}",
                appliedIds.Count,
                command.ResourceType,
                command.ResourceId);

            return new TagAssignmentResponse
            {
                Success = success,
                Message = message,
                ResourceId = command.ResourceId,
                AppliedTagIds = appliedIds,
                TotalTagsOnResource = allTags.Count(),
                Errors = errors
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error applying tags to resource");
            return new TagAssignmentResponse
            {
                Success = false,
                Message = "Error applying tags",
                ResourceId = command.ResourceId,
                AppliedTagIds = Enumerable.Empty<Guid>(),
                TotalTagsOnResource = 0,
                Errors = new[] { ex.Message }
            };
        }
    }
}

/// <summary>
/// Command handler for RemoveTagCommand.
/// </summary>
public class RemoveTagCommandHandler : ICommandHandler<RemoveTagCommand, TagAssignmentResponse>
{
    private readonly ITagService _tagService;
    private readonly ILogger<RemoveTagCommandHandler> _logger;

    public RemoveTagCommandHandler(ITagService tagService, ILogger<RemoveTagCommandHandler> logger)
    {
        _tagService = tagService;
        _logger = logger;
    }

    public async Task<TagAssignmentResponse> Handle(
        RemoveTagCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var removed = await _tagService.RemoveTagAsync(
                command.ResourceId,
                command.ResourceType,
                command.TagId,
                cancellationToken);

            var allTags = await _tagService.GetResourceTagsAsync(
                command.ResourceId,
                command.ResourceType,
                cancellationToken);

            _logger.LogInformation(
                "Removed tag {TagId} from {ResourceType} {ResourceId}",
                command.TagId,
                command.ResourceType,
                command.ResourceId);

            return new TagAssignmentResponse
            {
                Success = removed,
                Message = removed ? "Tag removed successfully" : "Tag was not applied to resource",
                ResourceId = command.ResourceId,
                AppliedTagIds = removed ? new[] { command.TagId } : Enumerable.Empty<Guid>(),
                TotalTagsOnResource = allTags.Count(),
                Errors = removed ? Enumerable.Empty<string>() : new[] { "Tag not found on resource" }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing tag from resource");
            return new TagAssignmentResponse
            {
                Success = false,
                Message = "Error removing tag",
                ResourceId = command.ResourceId,
                AppliedTagIds = Enumerable.Empty<Guid>(),
                TotalTagsOnResource = 0,
                Errors = new[] { ex.Message }
            };
        }
    }
}

/// <summary>
/// Command handler for SetResourceTagsCommand.
/// </summary>
public class SetResourceTagsCommandHandler : ICommandHandler<SetResourceTagsCommand, TagAssignmentResponse>
{
    private readonly ITagService _tagService;
    private readonly ILogger<SetResourceTagsCommandHandler> _logger;

    public SetResourceTagsCommandHandler(ITagService tagService, ILogger<SetResourceTagsCommandHandler> logger)
    {
        _tagService = tagService;
        _logger = logger;
    }

    public async Task<TagAssignmentResponse> Handle(
        SetResourceTagsCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var tagIdList = command.TagIds.ToList();
            var errors = new List<string>();

            // Validate all tags
            foreach (var tagId in tagIdList)
            {
                var tag = await _tagService.GetByIdAsync(tagId, cancellationToken);
                if (tag == null)
                    errors.Add($"Tag {tagId} not found");
                else if (!tag.CanBeUsedByService(command.ServiceName))
                    errors.Add($"Tag '{tag.Name}' cannot be used by service {command.ServiceName}");
            }

            if (errors.Any())
            {
                return new TagAssignmentResponse
                {
                    Success = false,
                    Message = $"Validation failed: {string.Join("; ", errors)}",
                    ResourceId = command.ResourceId,
                    AppliedTagIds = Enumerable.Empty<Guid>(),
                    TotalTagsOnResource = 0,
                    Errors = errors
                };
            }

            // Set tags (replaces all)
            var associations = await _tagService.SetResourceTagsAsync(
                command.ResourceId,
                command.ResourceType,
                tagIdList,
                command.ServiceName,
                command.AppliedBy,
                cancellationToken);

            _logger.LogInformation(
                "Set {Count} tags on {ResourceType} {ResourceId}",
                tagIdList.Count,
                command.ResourceType,
                command.ResourceId);

            return new TagAssignmentResponse
            {
                Success = true,
                Message = $"Successfully set {tagIdList.Count} tag(s)",
                ResourceId = command.ResourceId,
                AppliedTagIds = tagIdList,
                TotalTagsOnResource = associations.Count(),
                Errors = Enumerable.Empty<string>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error setting tags on resource");
            return new TagAssignmentResponse
            {
                Success = false,
                Message = "Error setting tags",
                ResourceId = command.ResourceId,
                AppliedTagIds = Enumerable.Empty<Guid>(),
                TotalTagsOnResource = 0,
                Errors = new[] { ex.Message }
            };
        }
    }
}

