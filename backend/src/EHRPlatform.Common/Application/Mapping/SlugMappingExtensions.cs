#nullable enable

using EHRPlatform.Common.Shared.DTOs;
using EHRPlatform.Common.Slugs;

namespace EHRPlatform.Common.Application.Common.Mapping;

/// <summary>
/// Extension methods for automatically applying slug values to DTOs during mapping.
/// Use with AutoMapper or manual mapping to add slug support.
/// </summary>
public static class SlugMappingExtensions
{
    /// <summary>
    /// Set slug on a SluggedResponseDto instance.
    /// </summary>
    public static T WithSlug<T>(this T dto, string? value) where T : SluggedResponseDto
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            dto.Slug = value;
            dto.SlugDisplayName = value.FromSlug();
        }
        return dto;
    }

    /// <summary>
    /// Set slug from a source value (auto-generates slug).
    /// </summary>
    public static T WithSlugFrom<T>(this T dto, string? value, ISlugGenerator? generator = null) where T : SluggedResponseDto
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            generator ??= new SlugGenerator();
            var slug = generator.Generate(value);
            dto.Slug = slug;
            dto.SlugDisplayName = value;
        }
        return dto;
    }

    /// <summary>
    /// Set slug and status slug on a StatusDto instance.
    /// </summary>
    public static T WithStatus<T>(this T dto, string? statusValue, ISlugGenerator? generator = null) where T : StatusDto
    {
        if (!string.IsNullOrWhiteSpace(statusValue))
        {
            generator ??= new SlugGenerator();
            dto.Status = statusValue;
            dto.StatusSlug = generator.Generate(statusValue);
        }
        return dto;
    }

    /// <summary>
    /// Batch-process DTOs to add slugs.
    /// </summary>
    public static List<T> WithSlugs<T>(this IEnumerable<T> dtos, Func<T, string?> slugSelector, ISlugGenerator? generator = null) where T : SluggedResponseDto
    {
        generator ??= new SlugGenerator();
        var result = new List<T>();

        foreach (var dto in dtos)
        {
            var slugValue = slugSelector(dto);
            if (!string.IsNullOrWhiteSpace(slugValue))
            {
                dto.Slug = generator.Generate(slugValue);
                dto.SlugDisplayName = slugValue;
            }
            result.Add(dto);
        }

        return result;
    }

    /// <summary>
    /// Create a slug map for a collection of DTOs for use in paged responses.
    /// Maps index position to slug value.
    /// </summary>
    public static Dictionary<int, string> CreateSlugMap<T>(
        this IEnumerable<T> dtos,
        Func<T, string?> slugSelector,
        ISlugGenerator? generator = null) where T : class
    {
        generator ??= new SlugGenerator();
        var slugMap = new Dictionary<int, string>();
        var index = 0;

        foreach (var dto in dtos)
        {
            var slugValue = slugSelector(dto);
            if (!string.IsNullOrWhiteSpace(slugValue))
            {
                var slug = generator.Generate(slugValue);
                slugMap[index] = slug;
            }
            index++;
        }

        return slugMap;
    }

    /// <summary>
    /// Convert PagedResult to PagedApiResponse with slug support.
    /// </summary>
    public static PagedApiResponse<T> ToPagedApiResponse<T>(
        this PagedResult<T> pagedResult,
        Func<T, string?>? slugSelector = null,
        ISlugGenerator? generator = null,
        string? correlationId = null) where T : class
    {
        var slugMap = slugSelector != null 
            ? pagedResult.Items.CreateSlugMap(slugSelector, generator)
            : null;

        return PagedApiResponse<T>.Success(pagedResult, slugMap, correlationId);
    }

    /// <summary>
    /// Convert single item to ApiResponse with slug support.
    /// </summary>
    public static ApiResponse<T> ToApiResponse<T>(
        this T data,
        Func<T, string?>? slugSelector = null,
        ISlugGenerator? generator = null,
        string? correlationId = null) where T : class
    {
        var slug = slugSelector != null
            ? (generator ?? new SlugGenerator()).Generate(slugSelector(data))
            : null;

        return ApiResponse<T>.Success(data, slug, correlationId);
    }
}

