using System;
using System.Text.RegularExpressions;

namespace EHRPlatform.SharedKernel.Domain.ValueObjects;

/// <summary>
/// Value object for phone number with international format support.
/// </summary>
public class PhoneNumber : ValueObject
{
    /// <summary>
    /// Phone number value (digits only).
    /// </summary>
    public string Value { get; }

    /// <summary>
    /// Formatted phone number (e.g., +1-555-123-4567).
    /// </summary>
    public string Formatted { get; }

    /// <summary>
    /// Country code (e.g., "+1" for US).
    /// </summary>
    public string CountryCode { get; }

    private PhoneNumber(string value, string formatted, string countryCode)
    {
        Value = value;
        Formatted = formatted;
        CountryCode = countryCode;
    }

    /// <summary>
    /// Create phone number with validation.
    /// </summary>
    public static Result<PhoneNumber> Create(string? phoneNumber, string countryCode = "+1")
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return Result<PhoneNumber>.Failure("Phone number is required");

        // Remove common separators
        var digits = Regex.Replace(phoneNumber, @"[^\d+]", "");

        // Validate length
        if (digits.Length < 10 || digits.Length > 15)
            return Result<PhoneNumber>.Failure("Phone number must be 10-15 digits");

        // Format phone number
        var formatted = FormatPhoneNumber(digits, countryCode);

        return Result<PhoneNumber>.Success(new PhoneNumber(digits, formatted, countryCode));
    }

    private static string FormatPhoneNumber(string digits, string countryCode)
    {
        // Remove + if present
        if (digits.StartsWith("+"))
            digits = digits[1..];

        // Basic US format: (XXX) XXX-XXXX
        if (digits.Length == 10)
            return $"{countryCode}-{digits[..3]}-{digits[3..6]}-{digits[6..]}";

        // International format
        return $"{countryCode}-{digits}";
    }

    protected override System.Collections.Generic.IEnumerable<object?> GetAtomicValues()
    {
        yield return Value;
    }

    public override string ToString() => Formatted;
}
