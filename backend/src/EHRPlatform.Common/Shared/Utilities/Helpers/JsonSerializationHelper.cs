#nullable enable

using System.Text.Json;
using System.Text.Json.Serialization;

namespace EHRPlatform.Common.Shared.Utilities.Helpers;

/// <summary>
/// Helper methods for JSON serialization with consistent configuration.
/// Centralizes JSON serialization options and patterns.
/// </summary>
public static class JsonSerializationHelper
{
    private static readonly Lazy<JsonSerializerOptions> DefaultOptions =
        new(() => CreateDefaultOptions());

    private static readonly Lazy<JsonSerializerOptions> CamelCaseOptions =
        new(() => CreateCamelCaseOptions());

    private static readonly Lazy<JsonSerializerOptions> IndentedOptions =
        new(() => CreateIndentedOptions());

    /// <summary>
    /// Get default JSON serialization options (case insensitive, no indentation).
    /// </summary>
    public static JsonSerializerOptions GetDefaultOptions()
    {
        return DefaultOptions.Value;
    }

    /// <summary>
    /// Get camelCase JSON serialization options.
    /// </summary>
    public static JsonSerializerOptions GetCamelCaseOptions()
    {
        return CamelCaseOptions.Value;
    }

    /// <summary>
    /// Get indented JSON serialization options (for logging/debugging).
    /// </summary>
    public static JsonSerializerOptions GetIndentedOptions()
    {
        return IndentedOptions.Value;
    }

    /// <summary>
    /// Serialize object to JSON string.
    /// </summary>
    public static string Serialize<T>(T? obj, JsonSerializerOptions? options = null)
    {
        if (obj == null)
            return "null";

        return JsonSerializer.Serialize(obj, options ?? GetDefaultOptions());
    }

    /// <summary>
    /// Serialize object to JSON string with indentation.
    /// </summary>
    public static string SerializeIndented<T>(T? obj)
    {
        if (obj == null)
            return "null";

        return JsonSerializer.Serialize(obj, GetIndentedOptions());
    }

    /// <summary>
    /// Deserialize JSON string to object.
    /// </summary>
    public static T? Deserialize<T>(string? json, JsonSerializerOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(json))
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(json, options ?? GetDefaultOptions());
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Deserialize JSON string to object of specified type.
    /// </summary>
    public static object? Deserialize(string? json, Type type, JsonSerializerOptions? options = null)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize(json, type, options ?? GetDefaultOptions());
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Deserialize JSON bytes to object.
    /// </summary>
    public static T? DeserializeFromBytes<T>(byte[]? bytes, JsonSerializerOptions? options = null)
    {
        if (bytes == null || bytes.Length == 0)
            return default;

        try
        {
            return JsonSerializer.Deserialize<T>(bytes, options ?? GetDefaultOptions());
        }
        catch
        {
            return default;
        }
    }

    /// <summary>
    /// Check if string is valid JSON.
    /// </summary>
    public static bool IsValidJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return false;

        try
        {
            using var document = JsonDocument.Parse(json);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Pretty-print JSON string (reformat with indentation).
    /// </summary>
    public static string? PrettyPrint(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return json;

        try
        {
            using var document = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(document, GetIndentedOptions());
        }
        catch
        {
            return json;
        }
    }

    /// <summary>
    /// Minify JSON string (remove whitespace).
    /// </summary>
    public static string? Minify(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return json;

        try
        {
            using var document = JsonDocument.Parse(json);
            return JsonSerializer.Serialize(document, GetDefaultOptions());
        }
        catch
        {
            return json;
        }
    }

    /// <summary>
    /// Get JSON element value by path (e.g., "user.name").
    /// </summary>
    public static string? GetJsonValueByPath(string? json, string path)
    {
        if (string.IsNullOrWhiteSpace(json) || string.IsNullOrWhiteSpace(path))
            return null;

        try
        {
            using var document = JsonDocument.Parse(json);
            var element = document.RootElement;

            foreach (var segment in path.Split('.'))
            {
                if (element.TryGetProperty(segment, out var child))
                {
                    element = child;
                }
                else
                {
                    return null;
                }
            }

            return element.GetRawText();
        }
        catch
        {
            return null;
        }
    }

    // ── Private Factory Methods ────────────────────────────────────────────

    private static JsonSerializerOptions CreateDefaultOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = null,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };
    }

    private static JsonSerializerOptions CreateCamelCaseOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Converters =
            {
                new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
            }
        };
    }

    private static JsonSerializerOptions CreateIndentedOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            PropertyNamingPolicy = null,
            Converters =
            {
                new JsonStringEnumConverter()
            }
        };
    }
}
