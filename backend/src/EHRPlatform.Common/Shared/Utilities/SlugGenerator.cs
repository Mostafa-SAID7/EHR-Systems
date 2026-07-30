#nullable enable

using System.Text.RegularExpressions;
using EHRPlatform.Common.Shared.Utilities;

namespace EHRPlatform.Common.Slugs;

/// <summary>
/// Default implementation of ISlugGenerator using StringHelper utilities.
/// Provides URL-friendly slug generation for entities.
/// </summary>
public sealed class SlugGenerator : ISlugGenerator
{
    /// <summary>
    /// Generate a slug from text.
    /// </summary>
    public string Generate(string? value, int? maxLength = null)
    {
        var slug = StringHelper.ToSlug(value);

        if (maxLength.HasValue && slug.Length > maxLength.Value)
            slug = slug.Substring(0, maxLength.Value).TrimEnd('-');

        return slug;
    }

    /// <summary>
    /// Generate a unique slug by appending numeric suffixes if needed.
    /// </summary>
    public async Task<string> GenerateUniqueAsync(string? value, Func<string, Task<bool>> existsCheck, int maxAttempts = 100)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Value cannot be empty", nameof(value));

        var baseSlug = Generate(value);

        // Check if base slug is available
        if (!await existsCheck(baseSlug))
            return baseSlug;

        // Try with numeric suffixes
        for (int i = 2; i <= maxAttempts; i++)
        {
            var candidateSlug = $"{baseSlug}-{i}";
            if (!await existsCheck(candidateSlug))
                return candidateSlug;
        }

        throw new InvalidOperationException($"Could not generate unique slug for '{value}' after {maxAttempts} attempts");
    }

    /// <summary>
    /// Parse slug back to readable text.
    /// </summary>
    public string Parse(string? slug)
    {
        return StringHelper.SlugToReadable(slug);
    }

    /// <summary>
    /// Validate if string is valid slug format.
    /// </summary>
    public bool IsValidSlug(string? slug)
    {
        if (string.IsNullOrWhiteSpace(slug))
            return false;

        // Valid slug: lowercase, alphanumeric, hyphens only
        return Regex.IsMatch(slug, @"^[a-z0-9]+(?:-[a-z0-9]+)*$", RegexOptions.Compiled);
    }

    /// <summary>
    /// Generate slug with prefix.
    /// </summary>
    public string GenerateWithPrefix(string? value, string? prefix)
    {
        if (string.IsNullOrWhiteSpace(prefix))
            return Generate(value);

        var slug = Generate(value);
        var cleanPrefix = Generate(prefix);

        return $"{cleanPrefix}-{slug}";
    }

    /// <summary>
    /// Create slug map for enum values.
    /// </summary>
    public Dictionary<string, string> CreateSlugMap(IEnumerable<string> values)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                var slug = Generate(value);
                if (!map.ContainsKey(slug))
                    map[slug] = value;
            }
        }

        return map;
    }
}

