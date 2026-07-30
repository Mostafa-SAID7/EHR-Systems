#nullable enable

namespace EHRPlatform.Common.Slugs;

/// <summary>
/// Service for generating and validating URL-friendly slugs.
/// Provides standardized slug generation across all services.
/// </summary>
public interface ISlugGenerator
{
    /// <summary>
    /// Generate a slug from text.
    /// </summary>
    /// <param name="value">Text to convert to slug (e.g., "First Name" → "first-name")</param>
    /// <param name="maxLength">Optional maximum slug length (default: no limit)</param>
    /// <returns>URL-friendly slug</returns>
    string Generate(string? value, int? maxLength = null);

    /// <summary>
    /// Generate a unique slug, checking for existing slugs using provided predicate.
    /// If slug already exists, appends incremental suffix (e.g., "first-name-2", "first-name-3")
    /// </summary>
    /// <param name="value">Text to convert</param>
    /// <param name="existsCheck">Async predicate to check if slug already exists</param>
    /// <param name="maxAttempts">Maximum attempts to find unique slug (default: 100)</param>
    /// <returns>Unique slug</returns>
    Task<string> GenerateUniqueAsync(string? value, Func<string, Task<bool>> existsCheck, int maxAttempts = 100);

    /// <summary>
    /// Parse a slug back to readable text.
    /// </summary>
    /// <param name="slug">URL-friendly slug (e.g., "first-name")</param>
    /// <returns>Readable text (e.g., "First Name")</returns>
    string Parse(string? slug);

    /// <summary>
    /// Validate if a string is a valid slug format.
    /// </summary>
    /// <param name="slug">Slug to validate</param>
    /// <returns>True if valid slug format</returns>
    bool IsValidSlug(string? slug);

    /// <summary>
    /// Generate slug with prefix (e.g., "INV-20250115-{slug}")
    /// </summary>
    /// <param name="value">Text to convert</param>
    /// <param name="prefix">Prefix to prepend with dash</param>
    /// <returns>Prefixed slug</returns>
    string GenerateWithPrefix(string? value, string? prefix);

    /// <summary>
    /// Create a slug map for enum or fixed set of values.
    /// </summary>
    /// <param name="values">Set of possible values</param>
    /// <returns>Dictionary mapping slug → value</returns>
    Dictionary<string, string> CreateSlugMap(IEnumerable<string> values);
}
