#nullable enable

using FluentValidation;

namespace EHRPlatform.Common.Slugs;

/// <summary>
/// Validator for slug format and constraints.
/// </summary>
public sealed class SlugValidator : AbstractValidator<string>
{
    private const int MaxSlugLength = 255;
    private const int MinSlugLength = 1;

    public SlugValidator()
    {
        RuleFor(s => s)
            .NotEmpty()
            .MinimumLength(MinSlugLength)
            .MaximumLength(MaxSlugLength)
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("Slug must be lowercase alphanumeric with hyphens only (e.g., 'first-name')");
    }
}

/// <summary>
/// Validation extension methods for slugs.
/// </summary>
public static class SlugValidationExtensions
{
    /// <summary>
    /// Add slug validation rule to FluentValidation rule builder.
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidSlug<T>(this IRuleBuilder<T, string?> rule)
    {
        return rule
            .NotEmpty()
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("'{PropertyName}' must be a valid slug (lowercase alphanumeric with hyphens)")
            .MaximumLength(255)
            .WithMessage("'{PropertyName}' must not exceed 255 characters");
    }

    /// <summary>
    /// Add slug format validation (without required constraint).
    /// </summary>
    public static IRuleBuilderOptions<T, string?> ValidSlugFormat<T>(this IRuleBuilder<T, string?> rule)
    {
        return rule
            .Matches(@"^[a-z0-9]+(?:-[a-z0-9]+)*$")
            .WithMessage("'{PropertyName}' must be a valid slug format (lowercase alphanumeric with hyphens)");
    }
}
