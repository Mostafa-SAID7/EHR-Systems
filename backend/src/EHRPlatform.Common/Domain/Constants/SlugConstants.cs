#nullable enable

namespace EHRPlatform.Common.Domain.Constants;

/// <summary>
/// Slug validation and generation constants.
/// Defines length limits and formatting rules for slugs.
/// Single responsibility: Define slug constants only.
/// </summary>
public static class SlugConstants
{
    /// <summary>Minimum slug length in characters.</summary>
    public const int MinLength = 1;

    /// <summary>Maximum slug length in characters.</summary>
    public const int MaxLength = 255;

    /// <summary>Pattern for valid slug characters (lowercase, hyphens, underscores, numbers).</summary>
    public const string ValidPattern = @"^[a-z0-9_-]+$";

    /// <summary>Separator character used in slugs.</summary>
    public const char Separator = '-';

    /// <summary>Maximum attempts to generate unique slug with numeric suffixes.</summary>
    public const int MaxUniqueAttempts = 100;
}
