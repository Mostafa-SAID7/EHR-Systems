using EHRPlatform.Common.Application.Features.TagManagement.Commands;
using EHRPlatform.Common.Shared.Contracts;

namespace EHRPlatform.Common.Application.Features.TagManagement.Validators;

/// <summary>
/// Validates tag assignment commands.
/// Single responsibility: Validation logic only.
/// </summary>
public class TagAssignmentValidator : ITagAssignmentValidator
{
    private readonly ITagService _tagService;

    public TagAssignmentValidator(ITagService tagService)
    {
        _tagService = tagService ?? throw new ArgumentNullException(nameof(tagService));
    }

    public async Task<TagValidationResult> ValidateApplyTagsAsync(
        ApplyTagsCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = new TagValidationResult();

        if (command.ResourceId == Guid.Empty)
            result.Errors.Add("ResourceId cannot be empty");

        if (string.IsNullOrWhiteSpace(command.ResourceType))
            result.Errors.Add("ResourceType cannot be empty");

        if (!command.TagIds.Any())
            result.Errors.Add("TagIds collection cannot be empty");

        foreach (var tagId in command.TagIds)
        {
            var tag = await _tagService.GetByIdAsync(tagId, cancellationToken);

            if (tag == null)
            {
                result.Errors.Add($"Tag {tagId} not found");
                continue;
            }

            if (!tag.CanBeUsedByService(command.ServiceName))
            {
                result.Errors.Add($"Tag '{tag.Name}' cannot be used by service {command.ServiceName}");
                continue;
            }

            result.ValidTagIds.Add(tagId);
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    public async Task<TagValidationResult> ValidateRemoveTagAsync(
        RemoveTagCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = new TagValidationResult();

        if (command.ResourceId == Guid.Empty)
            result.Errors.Add("ResourceId cannot be empty");

        var tag = await _tagService.GetByIdAsync(command.TagId, cancellationToken);

        if (tag == null)
            result.Errors.Add($"Tag {command.TagId} not found");
        else
            result.ValidTagIds.Add(command.TagId);

        result.IsValid = result.Errors.Count == 0;
        return result;
    }

    public async Task<TagValidationResult> ValidateSetResourceTagsAsync(
        SetResourceTagsCommand command,
        CancellationToken cancellationToken = default)
    {
        var result = new TagValidationResult();

        if (command.ResourceId == Guid.Empty)
            result.Errors.Add("ResourceId cannot be empty");

        if (string.IsNullOrWhiteSpace(command.ResourceType))
            result.Errors.Add("ResourceType cannot be empty");

        foreach (var tagId in command.TagIds)
        {
            var tag = await _tagService.GetByIdAsync(tagId, cancellationToken);

            if (tag == null)
            {
                result.Errors.Add($"Tag {tagId} not found");
                continue;
            }

            if (!tag.CanBeUsedByService(command.ServiceName))
            {
                result.Errors.Add($"Tag '{tag.Name}' cannot be used by service {command.ServiceName}");
                continue;
            }

            result.ValidTagIds.Add(tagId);
        }

        result.IsValid = result.Errors.Count == 0;
        return result;
    }
}
