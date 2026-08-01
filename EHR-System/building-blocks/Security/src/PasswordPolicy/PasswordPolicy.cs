using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace EHRPlatform.Security.PasswordPolicy;

/// <summary>
/// Implementation of password policy validation.
/// Single responsibility: Password strength validation.
/// </summary>
public class PasswordPolicy : IPasswordPolicy
{
    /// <summary>
    /// Validate password meets security requirements.
    /// </summary>
    public PasswordValidationResult Validate(string password)
    {
        var result = new PasswordValidationResult();

        if (string.IsNullOrWhiteSpace(password))
        {
            result.IsValid = false;
            result.Errors.Add("Password cannot be empty");
            return result;
        }

        var errors = new List<string>();
        int score = 0;

        // Minimum length (12 characters)
        if (password.Length < 12)
            errors.Add("Password must be at least 12 characters");
        else
            score += 20;

        // Uppercase letters
        if (Regex.IsMatch(password, "[A-Z]"))
            score += 20;
        else
            errors.Add("Password must contain at least one uppercase letter");

        // Lowercase letters
        if (Regex.IsMatch(password, "[a-z]"))
            score += 20;
        else
            errors.Add("Password must contain at least one lowercase letter");

        // Digits
        if (Regex.IsMatch(password, "[0-9]"))
            score += 20;
        else
            errors.Add("Password must contain at least one digit");

        // Special characters
        if (Regex.IsMatch(password, "[!@#$%^&*()_+\\-=\\[\\]{};':\"\\\\|,.<>?]"))
            score += 20;
        else
            errors.Add("Password must contain at least one special character (!@#$%^&*()...)");

        // Check for common patterns (not QWERTY, not 123456, etc.)
        var commonPatterns = new[] { "qwerty", "asdfgh", "zxcvbn", "123456", "000000", "password" };
        if (commonPatterns.Any(p => password.ToLower().Contains(p)))
        {
            errors.Add("Password contains common patterns or dictionary words");
            score = Math.Max(0, score - 20);
        }

        result.IsValid = errors.Count == 0;
        result.Errors = errors;
        result.StrengthScore = Math.Max(0, Math.Min(100, score));

        return result;
    }

    /// <summary>
    /// Get password policy requirements description.
    /// </summary>
    public string GetPolicyDescription()
    {
        return @"Password must meet the following requirements:
- Minimum 12 characters
- At least one uppercase letter (A-Z)
- At least one lowercase letter (a-z)
- At least one digit (0-9)
- At least one special character (!@#$%^&*()...)
- No common patterns (qwerty, 123456, password, etc.)";
    }
}
