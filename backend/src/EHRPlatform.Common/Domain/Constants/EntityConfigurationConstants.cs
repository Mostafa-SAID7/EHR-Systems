#nullable enable

namespace EHRPlatform.Common.Domain.Constants;

/// <summary>
/// Entity configuration constants for EF Core property builders.
/// Defines default string lengths, constraints, and limits.
/// Single responsibility: Define entity configuration constants only.
/// </summary>
public static class EntityConfigurationConstants
{
    /// <summary>Default maximum length for string properties (e.g., names, codes).</summary>
    public const int DefaultStringMaxLength = 255;

    /// <summary>Maximum length for email addresses.</summary>
    public const int EmailMaxLength = 254;

    /// <summary>Maximum length for phone numbers.</summary>
    public const int PhoneMaxLength = 20;

    /// <summary>Maximum length for URL/URI fields.</summary>
    public const int UrlMaxLength = 2048;

    /// <summary>Maximum length for country/region codes.</summary>
    public const int CountryCodeMaxLength = 2;

    /// <summary>Maximum length for currency codes.</summary>
    public const int CurrencyCodeMaxLength = 3;

    /// <summary>Maximum length for language codes.</summary>
    public const int LanguageCodeMaxLength = 5;

    /// <summary>Maximum length for status strings.</summary>
    public const int StatusMaxLength = 50;

    /// <summary>Maximum length for category names.</summary>
    public const int CategoryMaxLength = 100;

    /// <summary>Maximum length for description fields.</summary>
    public const int DescriptionMaxLength = 1000;

    /// <summary>Maximum length for notes/comments fields.</summary>
    public const int NotesMaxLength = 5000;

    /// <summary>Maximum length for JSON data fields.</summary>
    public const int JsonDataMaxLength = 65535;

    /// <summary>Default precision for decimal money fields.</summary>
    public const int MoneyPrecision = 18;

    /// <summary>Default scale for decimal money fields (2 decimal places).</summary>
    public const int MoneyScale = 2;
}
