namespace EHRPlatform.Services.Analytics.Domain.ValueObjects;

using System.Text.Json;

/// <summary>
/// Value object representing widget visualization configuration
/// Encapsulates chart settings, colors, thresholds, etc. as structured JSON
/// </summary>
public class WidgetConfiguration : IEquatable<WidgetConfiguration>
{
    /// <summary>
    /// Raw configuration JSON
    /// </summary>
    public string ConfigJson { get; private set; }

    /// <summary>
    /// Creates new WidgetConfiguration from JSON string
    /// </summary>
    /// <exception cref="JsonException">Thrown if JSON is invalid</exception>
    public WidgetConfiguration(string configJson)
    {
        // Validate JSON
        if (!string.IsNullOrWhiteSpace(configJson))
        {
            try
            {
                JsonDocument.Parse(configJson);
            }
            catch (JsonException ex)
            {
                throw new ArgumentException("Invalid JSON configuration", nameof(configJson), ex);
            }
        }

        ConfigJson = configJson ?? "{}";
    }

    /// <summary>
    /// Gets configuration as JsonDocument
    /// </summary>
    public JsonDocument GetJsonDocument() => JsonDocument.Parse(ConfigJson);

    /// <summary>
    /// Gets specific configuration property
    /// </summary>
    public JsonElement? GetProperty(string propertyName)
    {
        using var doc = GetJsonDocument();
        if (doc.RootElement.TryGetProperty(propertyName, out var property))
        {
            return property;
        }
        return null;
    }

    /// <summary>
    /// Creates configuration from dictionary
    /// </summary>
    public static WidgetConfiguration FromDictionary(Dictionary<string, object> config)
    {
        var json = JsonSerializer.Serialize(config);
        return new WidgetConfiguration(json);
    }

    /// <summary>
    /// Factory for empty configuration
    /// </summary>
    public static WidgetConfiguration Empty => new("{}");

    public bool Equals(WidgetConfiguration? other)
    {
        if (other is null) return false;
        return ConfigJson == other.ConfigJson;
    }

    public override bool Equals(object? obj) => Equals(obj as WidgetConfiguration);

    public override int GetHashCode() => ConfigJson.GetHashCode();

    public override string ToString() => ConfigJson;
}
