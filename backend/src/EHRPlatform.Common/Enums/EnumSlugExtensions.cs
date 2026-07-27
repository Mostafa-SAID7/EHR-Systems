#nullable enable

using EHRPlatform.Common.Slugs;

namespace EHRPlatform.Common.Enums;

/// <summary>
/// Extension methods for working with enums and the registry.
/// Provides unified slug generation and validation across all service enums.
/// </summary>
public static class EnumSlugExtensions
{
    /// <summary>
    /// Get slug representation of enum value.
    /// </summary>
    /// <example>
    /// AppointmentStatus.Scheduled.ToEnumSlug() → "scheduled"
    /// </example>
    public static string ToEnumSlug(this Enum value)
    {
        return value.ToString().ToSlug();
    }

    /// <summary>
    /// Parse string (or slug) back to enum value.
    /// Handles both "Scheduled" and "scheduled" formats.
    /// </summary>
    /// <example>
    /// "scheduled".TryParseEnum&lt;AppointmentStatus&gt;() → AppointmentStatus.Scheduled
    /// </example>
    public static bool TryParseEnum<T>(this string? value, out T result) where T : struct, Enum
    {
        result = default;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        // Try direct enum parse first
        if (Enum.TryParse<T>(value, ignoreCase: true, out var parsed))
        {
            result = parsed;
            return true;
        }

        // Try slug mapping
        var slugMap = value.ToEnumSlugMap<T>();
        var slugValue = value.ToSlug();

        if (slugMap.TryGetValue(slugValue, out var enumValue))
        {
            result = enumValue;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Get all enum values as slug → value map.
    /// </summary>
    /// <example>
    /// var map = typeof(AppointmentStatus).GetEnumSlugMap();
    /// // { "scheduled" → AppointmentStatus.Scheduled, ... }
    /// </example>
    public static Dictionary<string, T> ToEnumSlugMap<T>(this string? _) where T : struct, Enum
    {
        var map = new Dictionary<string, T>(StringComparer.OrdinalIgnoreCase);
        foreach (T value in Enum.GetValues(typeof(T)))
        {
            var slug = value.ToString().ToSlug();
            if (!map.ContainsKey(slug))
                map[slug] = value;
        }
        return map;
    }

    /// <summary>
    /// Validate if a string is a valid slug for the enum type.
    /// </summary>
    public static bool IsValidEnumSlug<T>(this string? value) where T : struct, Enum
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Enum.GetNames(typeof(T))
            .Any(name => name.ToSlug() == value.ToSlug());
    }

    /// <summary>
    /// Get metadata for an enum from the registry.
    /// </summary>
    public static EnumMetadata? GetMetadata(this Type enumType)
    {
        if (!enumType.IsEnum)
            return null;

        var fullName = enumType.FullName;
        return fullName != null ? EnumRegistry.Instance.GetMetadata(fullName) : null;
    }

    /// <summary>
    /// Validate enum value exists in registry.
    /// </summary>
    public static bool IsRegistered(this Type enumType)
    {
        return enumType.GetMetadata() != null;
    }
}
