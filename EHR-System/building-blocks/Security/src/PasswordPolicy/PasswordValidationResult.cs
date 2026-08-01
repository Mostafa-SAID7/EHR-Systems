using System.Collections.Generic;

namespace EHRPlatform.Security.PasswordPolicy;

/// <summary>
/// Result of password validation.
/// Single responsibility: Password validation result data structure.
/// </summary>
public class PasswordValidationResult
{
    /// <summary>
    /// Is password valid.
    /// </summary>
    public bool IsValid { get; set; }

    /// <summary>
    /// Validation errors if invalid.
    /// </summary>
    public List<string> Errors { get; set; } = new();

    /// <summary>
    /// Password strength score (0-100).
    /// </summary>
    public int StrengthScore { get; set; }
}
