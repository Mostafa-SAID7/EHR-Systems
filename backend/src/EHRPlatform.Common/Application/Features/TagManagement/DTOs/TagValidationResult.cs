#nullable enable

namespace EHRPlatform.Common.Application.Features.TagManagement.Validators;

/// <summary>
/// Validation result containing errors and valid tag IDs.
/// Single responsibility: Validation result transfer only.
/// </summary>
public class TagValidationResult
{
    /// <summary>
    /// Whether validation passed.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// List of validation errors (empty if valid).
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// List of tag IDs that passed validation.
    /// </summary>
    public List<Guid> ValidTagIds { get; set; } = new();
}
