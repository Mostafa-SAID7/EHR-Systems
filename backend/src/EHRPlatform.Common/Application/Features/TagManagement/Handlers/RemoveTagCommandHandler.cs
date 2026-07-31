using EHRPlatform.Common.Application.Common.CQRS;
using EHRPlatform.Common.Application.Features.TagManagement.Commands;
using EHRPlatform.Common.Application.Features.TagManagement.Responses;
using EHRPlatform.Common.Application.Features.TagManagement.Validators;
using EHRPlatform.Common.Shared.Contracts;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Common.Application.Features.TagManagement.Handlers;

/// <summary>
/// Handles RemoveTagCommand.
/// Single responsibility: Remove tag from resource.
/// </summary>
public class RemoveTagCommandHandler : ICommandHandler<RemoveTagCommand, TagAssignmentResponse>
{
    private readonly ITagService _tagService;
    private readonly ITagAssignmentValidator _validator;
    private readonly ILogger<RemoveTagCommandHandler> _logger;

    public RemoveTagCommandHandler(
        ITagService tagService,
        ITagAssignmentValidator validator,
        ILogger<RemoveTagCommandHandler> logger)
    {
        _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TagAssignmentResponse> Handle(
        RemoveTagCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _validator.ValidateRemoveTagAsync(command, cancellationToken);

            if (!validationResult.IsValid)
            {
                return new TagAssignmentResponse
                {
                    Success = false,
                    Message = $"Validation failed: {string.Join("; ", validationResult.Errors)}",
                    ResourceId = command.ResourceId,
                    Errors = validationResult.Errors
                };
            }

            var removed = await _tagService.RemoveTagAsync(
                command.ResourceId,
                command.ResourceType,
                command.TagId,
                cancellationToken);

            if (!removed)
            {
                return new TagAssignmentResponse
                {
                    Success = false,
                    Message = "Tag was not applied to resource",
                    ResourceId = command.ResourceId,
                    Errors = new[] { "Tag not found on resource" }
                };
            }

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
                Success = true,
                Message = "Tag removed successfully",
                ResourceId = command.ResourceId,
                AppliedTagIds = new[] { command.TagId },
                TotalTagsOnResource = allTags.Count()
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
                Errors = new[] { ex.Message }
            };
        }
    }
}
