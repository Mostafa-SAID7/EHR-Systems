using EHRPlatform.Common.Application.Features.TagManagement.Commands;

namespace EHRPlatform.Common.Application.Features.TagManagement.Validators;

/// <summary>
/// Validator abstraction for tag assignment operations.
/// Single responsibility: Define validation contract only.
/// </summary>
public interface ITagAssignmentValidator
{
    Task<TagValidationResult> ValidateApplyTagsAsync(
        ApplyTagsCommand command,
        CancellationToken cancellationToken = default);

    Task<TagValidationResult> ValidateRemoveTagAsync(
        RemoveTagCommand command,
        CancellationToken cancellationToken = default);

    Task<TagValidationResult> ValidateSetResourceTagsAsync(
        SetResourceTagsCommand command,
        CancellationToken cancellationToken = default);
}
