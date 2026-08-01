using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace EHRPlatform.Common.Extensions;

/// <summary>
/// Enumeration extensions for display names, descriptions, and conversions.
/// </summary>
public static class EnumExtensions
{
    /// <summary>
    /// Get the Display Name attribute value for an enum value.
    /// Falls back to ToString() if no attribute found.
    /// </summary>
    public static string GetDisplayName(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        if (field == null)
            return value.ToString();

        var displayAttribute = field.GetCustomAttribute<DisplayAttribute>();
        return displayAttribute?.Name ?? value.ToString();
    }

    /// <summary>
    /// Get the Description attribute value for an enum value.
    /// </summary>
    public static string GetDescription(this Enum value)
    {
        var field = value.GetType().GetField(value.ToString());
        if (field == null)
            return value.ToString();

        var displayAttribute = field.GetCustomAttribute<DisplayAttribute>();
        return displayAttribute?.Description ?? value.ToString();
    }

    /// <summary>
    /// Get all values of an enum type with their display names.
    /// </summary>
    public static IEnumerable<(T Value, string DisplayName)> GetValues<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T))
            .Cast<T>()
            .Select(e => (e, e.GetDisplayName()));
    }

    /// <summary>
    /// Get all enum values as a dictionary (value → display name).
    /// </summary>
    public static Dictionary<T, string> GetValueDictionary<T>() where T : Enum
    {
        return GetValues<T>()
            .ToDictionary(x => x.Value, x => x.DisplayName);
    }

    /// <summary>
    /// Get all enum values as a dictionary with descriptions.
    /// </summary>
    public static Dictionary<T, string> GetDescriptionDictionary<T>() where T : Enum
    {
        return Enum.GetValues(typeof(T))
            .Cast<T>()
            .ToDictionary(e => e, e => e.GetDescription());
    }

    /// <summary>
    /// Parse string to enum with display name support.
    /// </summary>
    public static bool TryParseEnum<T>(string value, out T result) where T : Enum
    {
        result = default!;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        // Try direct enum name
        if (Enum.TryParse<T>(value, ignoreCase: true, out var parsed))
        {
            result = parsed;
            return true;
        }

        // Try display name
        foreach (var field in typeof(T).GetFields())
        {
            var attr = field.GetCustomAttribute<DisplayAttribute>();
            if (attr?.Name?.Equals(value, StringComparison.OrdinalIgnoreCase) == true)
            {
                if (Enum.TryParse<T>(field.Name, out parsed))
                {
                    result = parsed;
                    return true;
                }
            }
        }

        return false;
    }

    /// <summary>
    /// Check if enum has a specific flag set.
    /// </summary>
    public static bool HasFlag<T>(this T value, T flag) where T : Enum
    {
        return ((dynamic)value & (dynamic)flag) != 0;
    }

    /// <summary>
    /// Get integer value of enum member.
    /// </summary>
    public static int ToInt(this Enum value)
    {
        return (int)(object)value;
    }

    /// <summary>
    /// Get long value of enum member.
    /// </summary>
    public static long ToLong(this Enum value)
    {
        return (long)(object)value;
    }
}
