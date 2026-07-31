#nullable enable

using System.Text;

namespace EHRPlatform.Common.Shared.Utilities.Helpers;

/// <summary>
/// Helper methods for type conversions and encoding.
/// Centralizes parsing, formatting, and encoding operations.
/// </summary>
public static class ConversionHelper
{
    /// <summary>
    /// Convert string to Base64 encoding.
    /// </summary>
    public static string ToBase64(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return string.Empty;

        var bytes = Encoding.UTF8.GetBytes(value);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Convert byte array to Base64 string.
    /// </summary>
    public static string ToBase64(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return string.Empty;

        return Convert.ToBase64String(bytes);
    }

    /// <summary>
    /// Decode Base64 string to UTF8 text.
    /// </summary>
    public static string? FromBase64(string? base64String)
    {
        if (string.IsNullOrEmpty(base64String))
            return null;

        try
        {
            var bytes = Convert.FromBase64String(base64String);
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Decode Base64 string to byte array.
    /// </summary>
    public static byte[]? FromBase64Bytes(string? base64String)
    {
        if (string.IsNullOrEmpty(base64String))
            return null;

        try
        {
            return Convert.FromBase64String(base64String);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Convert bytes to hexadecimal string.
    /// </summary>
    public static string ToHexString(byte[] bytes)
    {
        if (bytes == null || bytes.Length == 0)
            return string.Empty;

        return Convert.ToHexString(bytes);
    }

    /// <summary>
    /// Convert hexadecimal string to bytes.
    /// </summary>
    public static byte[]? FromHexString(string? hexString)
    {
        if (string.IsNullOrEmpty(hexString))
            return null;

        try
        {
            return Convert.FromHexString(hexString);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parse string to int safely.
    /// </summary>
    public static int? TryParseInt(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return int.TryParse(value, out var result) ? result : null;
    }

    /// <summary>
    /// Parse string to long safely.
    /// </summary>
    public static long? TryParseLong(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return long.TryParse(value, out var result) ? result : null;
    }

    /// <summary>
    /// Parse string to decimal safely.
    /// </summary>
    public static decimal? TryParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return decimal.TryParse(value, out var result) ? result : null;
    }

    /// <summary>
    /// Parse string to double safely.
    /// </summary>
    public static double? TryParseDouble(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return double.TryParse(value, out var result) ? result : null;
    }

    /// <summary>
    /// Parse string to boolean safely.
    /// </summary>
    public static bool? TryParseBool(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        if (bool.TryParse(value, out var result))
            return result;

        // Also support "1"/"0", "yes"/"no", "on"/"off"
        value = value.ToLower();
        return value switch
        {
            "1" or "yes" or "on" or "true" => true,
            "0" or "no" or "off" or "false" => false,
            _ => null
        };
    }

    /// <summary>
    /// Convert object to JSON string.
    /// </summary>
    public static string? ToJson<T>(T? obj) where T : class
    {
        if (obj == null)
            return null;

        try
        {
            return System.Text.Json.JsonSerializer.Serialize(obj);
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Parse JSON string to object.
    /// </summary>
    public static T? FromJson<T>(string? json) where T : class
    {
        if (string.IsNullOrEmpty(json))
            return null;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<T>(json);
        }
        catch
        {
            return null;
        }
    }
}
