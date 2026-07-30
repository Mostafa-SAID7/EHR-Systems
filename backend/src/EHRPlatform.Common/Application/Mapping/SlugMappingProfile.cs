#nullable enable

using EHRPlatform.Common.Shared.DTOs;
using EHRPlatform.Common.Slugs;

namespace EHRPlatform.Common.Application.Mapping;

/// <summary>
/// Helper class for manually applying slug values during DTO mapping.
/// Use this with your service's mapper to add slug support to response DTOs.
/// </summary>
public sealed class SlugMappingProfile
{
    private readonly ISlugGenerator _slugGenerator;

    public SlugMappingProfile(ISlugGenerator? slugGenerator = null)
    {
        _slugGenerator = slugGenerator ?? new SlugGenerator();
    }

    /// <summary>
    /// Apply slug value to a SluggedResponseDto.
    /// </summary>
    public SluggedResponseDto ApplySlug(SluggedResponseDto dto, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            dto.Slug = _slugGenerator.Generate(value);
            dto.SlugDisplayName = value;
        }
        return dto;
    }

    /// <summary>
    /// Apply status and statusSlug to a StatusDto.
    /// </summary>
    public StatusDto ApplyStatusSlug(StatusDto dto, string? statusValue)
    {
        if (!string.IsNullOrWhiteSpace(statusValue))
        {
            dto.Status = statusValue;
            dto.StatusSlug = _slugGenerator.Generate(statusValue);
        }
        return dto;
    }

    /// <summary>
    /// Batch-apply slugs to a collection of DTOs.
    /// </summary>
    public List<T> ApplySlugs<T>(IEnumerable<T> dtos, Func<T, string?> slugSelector) where T : SluggedResponseDto
    {
        var result = new List<T>();

        foreach (var dto in dtos)
        {
            var slugValue = slugSelector(dto);
            if (!string.IsNullOrWhiteSpace(slugValue))
            {
                dto.Slug = _slugGenerator.Generate(slugValue);
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
    public Dictionary<int, string> CreateSlugMap<T>(
        IEnumerable<T> items,
        Func<T, string?> slugSelector) where T : class
    {
        var slugMap = new Dictionary<int, string>();
        var index = 0;

        foreach (var item in items)
        {
            var slugValue = slugSelector(item);
            if (!string.IsNullOrWhiteSpace(slugValue))
            {
                var slug = _slugGenerator.Generate(slugValue);
                slugMap[index] = slug;
            }
            index++;
        }

        return slugMap;
    }

    /// <summary>
    /// Convert PagedResult to PagedApiResponse with slug support.
    /// </summary>
    public PagedApiResponse<T> CreatePagedResponse<T>(
        PagedResult<T> pagedResult,
        Func<T, string?>? slugSelector = null,
        string? correlationId = null) where T : class
    {
        var slugMap = slugSelector != null 
            ? CreateSlugMap(pagedResult.Items, slugSelector)
            : null;

        return PagedApiResponse<T>.Success(pagedResult, slugMap, correlationId);
    }
}

