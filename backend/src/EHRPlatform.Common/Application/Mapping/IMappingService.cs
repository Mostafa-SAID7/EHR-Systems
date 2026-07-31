#nullable enable

namespace EHRPlatform.Common.Application.Common.Mapping;

/// <summary>
/// Interface for mapping services.
/// Provides abstraction for entity-to-DTO conversions.
/// </summary>
public interface IMappingService
{
    /// <summary>
    /// Map single entity to DTO.
    /// </summary>
    TDto MapToDto<TEntity, TDto>(TEntity entity) where TEntity : class where TDto : class;

    /// <summary>
    /// Map collection of entities to DTOs.
    /// </summary>
    IEnumerable<TDto> MapToDtoList<TEntity, TDto>(IEnumerable<TEntity> entities) 
        where TEntity : class where TDto : class;

    /// <summary>
    /// Map DTO to entity.
    /// </summary>
    TEntity MapToEntity<TDto, TEntity>(TDto dto) where TEntity : class where TDto : class;
}

