#nullable enable

namespace EHRPlatform.Common.Slugs;

/// <summary>
/// Extension methods for slug operations on common types.
/// </summary>
public static class SlugExtensions
{
    private static readonly ISlugGenerator DefaultGenerator = new SlugGenerator();

    /// <summary>
    /// Convert string to slug.
    /// </summary>
    public static string ToSlug(this string? value, int? maxLength = null)
    {
        return DefaultGenerator.Generate(value, maxLength);
    }

    /// <summary>
    /// Convert slug to readable text.
    /// </summary>
    public static string FromSlug(this string? slug)
    {
        return DefaultGenerator.Parse(slug);
    }

    /// <summary>
    /// Check if string is valid slug format.
    /// </summary>
    public static bool IsValidSlug(this string? slug)
    {
        return DefaultGenerator.IsValidSlug(slug);
    }

    /// <summary>
    /// Generate slug with prefix.
    /// </summary>
    public static string SlugWithPrefix(this string? value, string? prefix)
    {
        return DefaultGenerator.GenerateWithPrefix(value, prefix);
    }

    /// <summary>
    /// Generate slug from enum value (e.g., AppointmentType.Office → "office").
    /// </summary>
    public static string ToEnumSlug(this Enum enumValue)
    {
        return enumValue.ToString().ToSlug();
    }

    /// <summary>
    /// Get enum value from slug (e.g., "office" → AppointmentType.Office).
    /// </summary>
    public static T? FromEnumSlug<T>(this string? slug) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(slug))
            return null;

        var values = Enum.GetValues(typeof(T));
        foreach (T value in values)
        {
            if (value.ToString().ToSlug() == slug)
                return value;
        }

        return null;
    }

    /// <summary>
    /// Get all enum values as slug dictionary.
    /// </summary>
    public static Dictionary<string, T> ToEnumSlugMap<T>() where T : struct, Enum
    {
        var map = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        foreach (T value in Enum.GetValues(typeof(T)))
        {
            var slug = value.ToString().ToSlug();
            map[slug] = value;
        }
        return map;
    }

    /// <summary>
    /// Get all enum values as slug → name dictionary (strings).
    /// </summary>
    public static Dictionary<string, string> ToEnumSlugStringMap<T>() where T : struct, Enum
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (T value in Enum.GetValues(typeof(T)))
        {
            var slug = value.ToString().ToSlug();
            map[slug] = value.ToString();
        }
        return map;
    }

    /// <summary>
    /// Extract slug value from enum, handling duplicates.
    /// </summary>
    public static string GetSlug<T>(this T enumValue) where T : struct, Enum
    {
        return enumValue.ToString().ToSlug();
    }
}
