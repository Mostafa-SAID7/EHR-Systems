#nullable enable

namespace EHRPlatform.Common.Domain.Constants;

/// <summary>
/// String handling and formatting constants.
/// Defines string length limits, truncation, and character sets.
/// Single responsibility: Define string constants only.
/// </summary>
public static class StringConstants
{
    /// <summary>Default maximum length for string truncation.</summary>
    public const int DefaultTruncateLength = 50;

    /// <summary>Ellipsis string appended when truncating.</summary>
    public const string EllipsisMarker = "...";

    /// <summary>Length of ellipsis marker.</summary>
    public const int EllipsisLength = 3;

    // ── Character sets for random string generation ─────────────────────

    /// <summary>Lowercase letter characters.</summary>
    public const string LowercaseChars = "abcdefghijklmnopqrstuvwxyz";

    /// <summary>Uppercase letter characters.</summary>
    public const string UppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    /// <summary>Numeric digit characters.</summary>
    public const string NumberChars = "0123456789";

    /// <summary>Special characters for random string generation.</summary>
    public const string SpecialChars = "!@#$%^&*()_+-=[]{}|;:,.<>?";

    /// <summary>Default length for random string generation.</summary>
    public const int DefaultRandomLength = 10;
}
