using EHRPlatform.Common.Application.Common.CQRS;
using EHRPlatform.Common.Application.Features.TagManagement.Commands;
using EHRPlatform.Common.Application.Features.TagManagement.Responses;
using EHRPlatform.Common.Application.Features.TagManagement.Validators;
using EHRPlatform.Common.Shared.Contracts;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Common.Application.Features.TagManagement.Handlers;

/// <summary>
/// Handles ApplyTagsCommand.
/// Single responsibility: Apply tags to resource.
/// </summary>
public class ApplyTagsCommandHandler : ICommandHandler<ApplyTagsCommand, TagAssignmentResponse>
{
    private readonly ITagService _tagService;
    private readonly ITagAssignmentValidator _validator;
    private readonly ILogger<ApplyTagsCommandHandler> _logger;

    public ApplyTagsCommandHandler(
        ITagService tagService,
        ITagAssignmentValidator validator,
        ILogger<ApplyTagsCommandHandler> logger)
    {
        _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TagAssignmentResponse> Handle(
        ApplyTagsCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _validator.ValidateApplyTagsAsync(command, cancellationToken);

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

            foreach (var tagId in validationResult.ValidTagIds)
            {
                await _tagService.ApplyTagAsync(
                    command.ResourceId,
                    command.ResourceType,
                    tagId,
                    command.ServiceName,
                    command.Context,
                    command.AppliedBy,
                    cancellationToken);
            }

            var allTags = await _tagService.GetResourceTagsAsync(
                command.ResourceId,
                command.ResourceType,
                cancellationToken);

            _logger.LogInformation(
                "Applied {Count} tags to {ResourceType} {ResourceId}",
                validationResult.ValidTagIds.Count,
                command.ResourceType,
                command.ResourceId);

            return new TagAssignmentResponse
            {
                Success = true,
                Message = $"Successfully applied {validationResult.ValidTagIds.Count} tag(s)",
                ResourceId = command.ResourceId,
                AppliedTagIds = validationResult.ValidTagIds,
                TotalTagsOnResource = allTags.Count()
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
                Errors = new[] { ex.Message }
            };
        }
    }
}
