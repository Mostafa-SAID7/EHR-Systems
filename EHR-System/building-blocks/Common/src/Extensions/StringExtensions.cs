using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace EHRPlatform.Common.Extensions;

/// <summary>
/// String manipulation and validation extensions for healthcare data processing.
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Slug a string for URL-safe identifiers (e.g., "Clinical Note" → "clinical-note").
    /// </summary>
    public static string ToSlug(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        // Convert to lowercase
        string slug = value.ToLowerInvariant();

        // Replace spaces with hyphens
        slug = Regex.Replace(slug, @"\s+", "-");

        // Remove invalid URL characters
        slug = Regex.Replace(slug, @"[^a-z0-9\-]", "");

        // Replace multiple hyphens with single
        slug = Regex.Replace(slug, @"-+", "-");

        // Trim leading/trailing hyphens
        return slug.Trim('-');
    }

    /// <summary>
    /// Capitalize first letter of a string.
    /// </summary>
    public static string Capitalize(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        return char.ToUpper(value[0]) + value[1..];
    }

    /// <summary>
    /// Convert PascalCase to human-readable text (e.g., "PatientId" → "Patient Id").
    /// </summary>
    public static string ToHumanReadable(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return value;

        var result = Regex.Replace(value, @"([A-Z])", " $1");
        return result.Trim().Capitalize();
    }

    /// <summary>
    /// Check if string is a valid email format.
    /// </summary>
    public static bool IsValidEmail(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        try
        {
            var addr = new System.Net.Mail.MailAddress(value);
            return addr.Address == value;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Check if string is a valid phone number format (basic validation).
    /// </summary>
    public static bool IsValidPhoneNumber(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        // Remove common phone separators
        var digits = Regex.Replace(value, @"[^\d+]", "");

        // Must be between 10-15 digits (international format)
        return digits.Length is >= 10 and <= 15;
    }

    /// <summary>
    /// Truncate string to maximum length with ellipsis.
    /// </summary>
    public static string Truncate(this string value, int maxLength = 100, string suffix = "...")
    {
        if (string.IsNullOrEmpty(value))
            return value;

        if (value.Length <= maxLength)
            return value;

        return value[..maxLength] + suffix;
    }

    /// <summary>
    /// Remove extra whitespace from string.
    /// </summary>
    public static string NormalizeWhitespace(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        return Regex.Replace(value.Trim(), @"\s+", " ");
    }

    /// <summary>
    /// Mask sensitive data for logging (e.g., "user@example.com" → "u***@example.com").
    /// </summary>
    public static string MaskSensitive(this string value, int visibleChars = 1)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= visibleChars)
            return "***";

        return value[..visibleChars] + new string('*', value.Length - visibleChars);
    }

    /// <summary>
    /// Extract numbers from string.
    /// </summary>
    public static string ExtractNumbers(this string value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        return Regex.Replace(value, @"[^\d]", "");
    }

    /// <summary>
    /// Check if string contains only alphabetic characters and spaces.
    /// </summary>
    public static bool IsAlphaOnly(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Regex.IsMatch(value, @"^[a-zA-Z\s]+$");
    }

    /// <summary>
    /// Repeat string n times.
    /// </summary>
    public static string Repeat(this string value, int count)
    {
        if (count <= 0 || string.IsNullOrEmpty(value))
            return string.Empty;

        var sb = new StringBuilder(value.Length * count);
        for (int i = 0; i < count; i++)
            sb.Append(value);

        return sb.ToString();
    }

    /// <summary>
    /// Check if string matches HIPAA medical record number format (typically alphanumeric).
    /// </summary>
    public static bool IsValidMedicalRecordNumber(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        // MRN format: alphanumeric, 8-20 characters
        return Regex.IsMatch(value, @"^[A-Z0-9]{8,20}$");
    }

    /// <summary>
    /// Check if string is a valid ICD-10 code (e.g., "A00" or "A00.0").
    /// </summary>
    public static bool IsValidICD10Code(this string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        // ICD-10: Letter followed by 2 digits, optionally followed by decimal and characters
        return Regex.IsMatch(value, @"^[A-Z]\d{2}(\.\d+)?$");
    }
}
