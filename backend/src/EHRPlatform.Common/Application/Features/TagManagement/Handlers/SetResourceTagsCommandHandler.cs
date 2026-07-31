using EHRPlatform.Common.Application.Common.CQRS;
using EHRPlatform.Common.Application.Features.TagManagement.Commands;
using EHRPlatform.Common.Application.Features.TagManagement.Responses;
using EHRPlatform.Common.Application.Features.TagManagement.Validators;
using EHRPlatform.Common.Shared.Contracts;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Common.Application.Features.TagManagement.Handlers;

/// <summary>
/// Handles SetResourceTagsCommand.
/// Single responsibility: Set all tags on resource.
/// </summary>
public class SetResourceTagsCommandHandler : ICommandHandler<SetResourceTagsCommand, TagAssignmentResponse>
{
    private readonly ITagService _tagService;
    private readonly ITagAssignmentValidator _validator;
    private readonly ILogger<SetResourceTagsCommandHandler> _logger;

    public SetResourceTagsCommandHandler(
        ITagService tagService,
        ITagAssignmentValidator validator,
        ILogger<SetResourceTagsCommandHandler> logger)
    {
        _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
        _validator = validator ?? throw new ArgumentNullException(nameof(validator));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<TagAssignmentResponse> Handle(
        SetResourceTagsCommand command,
        CancellationToken cancellationToken)
    {
        try
        {
            var validationResult = await _validator.ValidateSetResourceTagsAsync(command, cancellationToken);

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

            await _tagService.SetResourceTagsAsync(
                command.ResourceId,
                command.ResourceType,
                validationResult.ValidTagIds,
                command.ServiceName,
                command.AppliedBy,
                cancellationToken);

            _logger.LogInformation(
                "Set {Count} tags on {ResourceType} {ResourceId}",
                validationResult.ValidTagIds.Count,
                command.ResourceType,
                command.ResourceId);

            return new TagAssignmentResponse
            {
                Success = true,
                Message = $"Successfully set {validationResult.ValidTagIds.Count} tag(s)",
                ResourceId = command.ResourceId,
                AppliedTagIds = validationResult.ValidTagIds,
                TotalTagsOnResource = validationResult.ValidTagIds.Count
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
                Errors = new[] { ex.Message }
            };
        }
    }
}
