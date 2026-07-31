#nullable enable

using System.Text;
using System.Text.RegularExpressions;

namespace EHRPlatform.Common.Shared.Utilities.Helpers;

/// <summary>
/// Helper methods for string operations and formatting.
/// Use across all services for consistency.
/// </summary>
public static class StringHelper
{
    /// <summary>
    /// Check if string is null, empty, or whitespace.
    /// </summary>
    public static bool IsNullOrEmpty(string? value)
    {
        return string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Check if string has a value (not null, empty, or whitespace).
    /// </summary>
    public static bool HasValue(string? value)
    {
        return !string.IsNullOrWhiteSpace(value);
    }

    /// <summary>
    /// Truncate string to max length and add ellipsis if needed.
    /// </summary>
    public static string Truncate(string? value, int maxLength = 50)
    {
        if (IsNullOrEmpty(value))
            return string.Empty;

        if (value!.Length <= maxLength)
            return value;

        return value.Substring(0, maxLength - 3) + "...";
    }

    /// <summary>
    /// Convert string to title case (capitalize each word).
    /// </summary>
    public static string ToTitleCase(string? value)
    {
        if (IsNullOrEmpty(value))
            return string.Empty;

        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(value!.ToLower());
    }

    /// <summary>
    /// Convert CamelCase or PascalCase to readable format (e.g., FirstName -> First Name).
    /// </summary>
    public static string ToReadableFormat(string? value)
    {
        if (IsNullOrEmpty(value))
            return string.Empty;

        var result = Regex.Replace(value!, "([A-Z])", " $1", RegexOptions.Compiled).Trim();
        return System.Globalization.CultureInfo.CurrentCulture.TextInfo.ToTitleCase(result);
    }

    /// <summary>
    /// Remove all whitespace from string.
    /// </summary>
    public static string RemoveWhitespace(string? value)
    {
        if (IsNullOrEmpty(value))
            return string.Empty;

        return Regex.Replace(value!, @"\s+", "", RegexOptions.Compiled);
    }

    /// <summary>
    /// Sanitize string by removing special characters.
    /// </summary>
    public static string Sanitize(string? value, bool allowSpaces = true)
    {
        if (IsNullOrEmpty(value))
            return string.Empty;

        var pattern = allowSpaces ? "[^a-zA-Z0-9 -]" : "[^a-zA-Z0-9-]";
        return Regex.Replace(value!, pattern, "", RegexOptions.Compiled);
    }

    /// <summary>
    /// Check if string matches email pattern (basic validation).
    /// </summary>
    public static bool IsValidEmail(string? value)
    {
        if (IsNullOrEmpty(value))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(value!);
            return addr.Address == value;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Check if string is a valid phone number (US format).
    /// </summary>
    public static bool IsValidPhoneNumber(string? value)
    {
        if (IsNullOrEmpty(value))
            return false;

        var pattern = @"^\+?1?\d{9,15}$";
        return Regex.IsMatch(value!, pattern, RegexOptions.Compiled);
    }

    /// <summary>
    /// Check if string contains only numbers.
    /// </summary>
    public static bool IsNumeric(string? value)
    {
        if (IsNullOrEmpty(value))
            return false;

        return long.TryParse(value!, out _);
    }

    /// <summary>
    /// Check if string contains only letters.
    /// </summary>
    public static bool IsAlpha(string? value)
    {
        if (IsNullOrEmpty(value))
            return false;

        return Regex.IsMatch(value!, @"^[a-zA-Z]+$", RegexOptions.Compiled);
    }

    /// <summary>
    /// Check if string contains only alphanumeric characters.
    /// </summary>
    public static bool IsAlphaNumeric(string? value)
    {
        if (IsNullOrEmpty(value))
            return false;

        return Regex.IsMatch(value!, @"^[a-zA-Z0-9]+$", RegexOptions.Compiled);
    }

    /// <summary>
    /// Reverse a string.
    /// </summary>
    public static string Reverse(string? value)
    {
        if (IsNullOrEmpty(value))
            return string.Empty;

        var chars = value!.ToCharArray();
        Array.Reverse(chars);
        return new string(chars);
    }

    /// <summary>
    /// Get initials from a full name (e.g., "John Doe" -> "JD").
    /// </summary>
    public static string GetInitials(string? fullName)
    {
        if (IsNullOrEmpty(fullName))
            return string.Empty;

        var parts = fullName!.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var initials = string.Concat(parts.Select(p => p[0].ToString().ToUpper()));
        return initials;
    }

    /// <summary>
    /// Mask sensitive data (e.g., "john@example.com" -> "j***@example.com").
    /// </summary>
    public static string MaskEmail(string? email)
    {
        if (IsNullOrEmpty(email) || !email!.Contains("@"))
            return string.Empty;

        var parts = email!.Split('@');
        var localPart = parts[0];
        var domain = parts[1];

        if (localPart.Length < 2)
            return email;

        var masked = localPart[0] + new string('*', localPart.Length - 2) + localPart[^1];
        return $"{masked}@{domain}";
    }

    /// <summary>
    /// Mask phone number (e.g., "1234567890" -> "***-***-7890").
    /// </summary>
    public static string MaskPhoneNumber(string? phoneNumber)
    {
        if (IsNullOrEmpty(phoneNumber) || phoneNumber!.Length < 4)
            return string.Empty;

        var digits = Regex.Replace(phoneNumber!, @"\D", "");
        if (digits.Length < 4)
            return digits;

        return "***-***-" + digits[^4..];
    }

    /// <summary>
    /// Generate a random string of specified length.
    /// </summary>
    public static string GenerateRandomString(int length = 10, bool includeUpperCase = true, bool includeNumbers = true)
    {
        const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
        const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string numbers = "0123456789";

        var chars = lowerCase;
        if (includeUpperCase) chars += upperCase;
        if (includeNumbers) chars += numbers;

        var random = new Random();
        var result = new StringBuilder();

        for (int i = 0; i < length; i++)
            result.Append(chars[random.Next(chars.Length)]);

        return result.ToString();
    }

    /// <summary>
    /// Convert a slug back to readable text (e.g., "first-name" -> "First Name").
    /// </summary>
    public static string SlugToReadable(string? slug)
    {
        if (IsNullOrEmpty(slug))
            return string.Empty;

        var text = slug!.Replace("-", " ").Replace("_", " ");
        return ToTitleCase(text);
    }

    /// <summary>
    /// Convert text to URL-friendly slug (e.g., "First Name" -> "first-name").
    /// </summary>
    public static string ToSlug(string? text)
    {
        if (IsNullOrEmpty(text))
            return string.Empty;

        var slug = text!
            .ToLower()
            .Replace(" ", "-")
            .Replace("_", "-");

        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "", RegexOptions.Compiled);
        slug = Regex.Replace(slug, @"-+", "-", RegexOptions.Compiled);

        return slug.Trim('-');
    }

    /// <summary>
    /// Left-pad a string with a character.
    /// </summary>
    public static string LeftPad(string? value, int width, char paddingChar = ' ')
    {
        if (IsNullOrEmpty(value))
            return new string(paddingChar, width);

        return value!.PadLeft(width, paddingChar);
    }

    /// <summary>
    /// Right-pad a string with a character.
    /// </summary>
    public static string RightPad(string? value, int width, char paddingChar = ' ')
    {
        if (IsNullOrEmpty(value))
            return new string(paddingChar, width);

        return value!.PadRight(width, paddingChar);
    }
}

