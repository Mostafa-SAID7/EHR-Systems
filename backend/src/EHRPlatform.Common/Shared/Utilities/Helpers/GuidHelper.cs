#nullable enable

namespace EHRPlatform.Common.Shared.Utilities.Helpers;

/// <summary>
/// Helper methods for GUID operations and formatting.
/// Centralizes Guid creation, parsing, and validation.
/// </summary>
public static class GuidHelper
{
    /// <summary>
    /// Generate a new GUID.
    /// </summary>
    public static Guid NewGuid() => Guid.NewGuid();

    /// <summary>
    /// Create a GUID from a string value.
    /// </summary>
    public static Guid Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentNullException(nameof(value), "GUID string cannot be null or empty.");

        return Guid.Parse(value);
    }

    /// <summary>
    /// Try to parse a string to GUID.
    /// </summary>
    public static bool TryParse(string? value, out Guid result)
    {
        result = Guid.Empty;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Guid.TryParse(value, out result);
    }

    /// <summary>
    /// Check if GUID is empty/default.
    /// </summary>
    public static bool IsEmpty(Guid value)
    {
        return value == Guid.Empty;
    }

    /// <summary>
    /// Check if GUID is not empty.
    /// </summary>
    public static bool IsNotEmpty(Guid value)
    {
        return value != Guid.Empty;
    }

    /// <summary>
    /// Format GUID as string (default: D = 32 digits with hyphens).
    /// </summary>
    public static string Format(Guid value, string format = "D")
    {
        return value.ToString(format);
    }

    /// <summary>
    /// Format GUID as string without hyphens (N format).
    /// </summary>
    public static string FormatCompact(Guid value)
    {
        return value.ToString("N");
    }

    /// <summary>
    /// Create a deterministic GUID from a namespace and name (v5 SHA-1).
    /// </summary>
    public static Guid CreateNameBasedGuid(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentNullException(nameof(name));

        // Use SHA-1 namespace for deterministic GUIDs
        const string ns = "6ba7b810-9dad-11d1-80b4-00c04fd430c8"; // URL namespace
        return CreateNameBasedGuidv5(Guid.Parse(ns), name);
    }

    /// <summary>
    /// Create v5 GUID from namespace and name using SHA-1.
    /// </summary>
    private static Guid CreateNameBasedGuidv5(Guid namespaceGuid, string name)
    {
        var namespaceBytes = namespaceGuid.ToByteArray();
        var nameBytes = System.Text.Encoding.UTF8.GetBytes(name);

        using var sha1 = System.Security.Cryptography.SHA1.Create();
        var hash = sha1.ComputeHash(namespaceBytes.Concat(nameBytes).ToArray());

        // Set version to 5 (SHA-1) and variant to RFC 4122
        hash[6] = (byte)((hash[6] & 0x0f) | 0x50);
        hash[8] = (byte)((hash[8] & 0x3f) | 0x80);

        return new Guid(hash.Take(16).ToArray());
    }
}
