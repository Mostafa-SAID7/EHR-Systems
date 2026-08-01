namespace EHRPlatform.Common.Configuration;

/// <summary>
/// Interface for application configuration access.
/// Single responsibility: Configuration provider contract.
/// </summary>
public interface IConfigurationProvider
{
    /// <summary>
    /// Get configuration value as string.
    /// </summary>
    string? GetString(string key);

    /// <summary>
    /// Get configuration value as string with default.
    /// </summary>
    string GetString(string key, string defaultValue);

    /// <summary>
    /// Get configuration value as int.
    /// </summary>
    int? GetInt(string key);

    /// <summary>
    /// Get configuration value as int with default.
    /// </summary>
    int GetInt(string key, int defaultValue);

    /// <summary>
    /// Get configuration value as bool.
    /// </summary>
    bool? GetBool(string key);

    /// <summary>
    /// Get configuration value as bool with default.
    /// </summary>
    bool GetBool(string key, bool defaultValue);

    /// <summary>
    /// Get configuration section as object.
    /// </summary>
    T? GetSection<T>(string key) where T : class;

    /// <summary>
    /// Check if key exists.
    /// </summary>
    bool KeyExists(string key);
}
