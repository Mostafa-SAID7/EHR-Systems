#nullable enable

namespace EHRPlatform.Common.Application.Mapping;

using Mapster;
using Microsoft.Extensions.Logging;

/// <summary>
/// Base class for entity mappers.
/// Provides common mapping functionality with logging and error handling.
/// Single Responsibility: Each mapper handles ONE entity type.
/// 
/// This class provides convenience methods for mapping a specific TEntity/TDto pair.
/// For generic mapping, use IMappingService directly or create mapper implementations per entity type.
/// 
/// Usage:
/// public class PatientMappingService : MappingServiceBase<Patient, PatientDto>
/// {
///     public PatientMappingService(ILogger<MappingServiceBase<Patient, PatientDto>> logger) : base(logger) { }
/// }
/// 
/// // Later in handlers/queries
/// var patientDto = _mapper.MapSingleToDto(patient);
/// var patientDtos = _mapper.MapListToDto(patients);
/// var patient = _mapper.MapSingleToEntity(patientDto);
/// </summary>
public abstract class MappingServiceBase<TEntity, TDto>
    where TEntity : class
    where TDto : class
{
    protected readonly ILogger<MappingServiceBase<TEntity, TDto>> Logger;

    protected MappingServiceBase(ILogger<MappingServiceBase<TEntity, TDto>> logger)
    {
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Map single entity to DTO.
    /// </summary>
    protected virtual TDto MapSingleToDto(TEntity entity)
    {
        if (entity == null)
        {
            Logger.LogWarning("Attempted to map null entity of type {EntityType}", typeof(TEntity).Name);
            throw new ArgumentNullException(nameof(entity));
        }

        try
        {
            var dto = entity.Adapt<TDto>();
            Logger.LogDebug("Successfully mapped {EntityType} to {DtoType}", typeof(TEntity).Name, typeof(TDto).Name);
            return dto;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error mapping {EntityType} to {DtoType}", typeof(TEntity).Name, typeof(TDto).Name);
            throw;
        }
    }

    /// <summary>
    /// Map collection of entities to DTOs.
    /// </summary>
    protected virtual IEnumerable<TDto> MapListToDto(IEnumerable<TEntity> entities)
    {
        if (entities == null)
        {
            Logger.LogWarning("Attempted to map null entity collection");
            throw new ArgumentNullException(nameof(entities));
        }

        try
        {
            var entityList = entities.ToList();
            var dtoList = entityList.Adapt<List<TDto>>();
            Logger.LogDebug("Successfully mapped {Count} entities of type {EntityType} to {DtoType}", 
                entityList.Count, typeof(TEntity).Name, typeof(TDto).Name);
            return dtoList;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error mapping entity collection of type {EntityType} to {DtoType}", 
                typeof(TEntity).Name, typeof(TDto).Name);
            throw;
        }
    }

    /// <summary>
    /// Map DTO to entity (for updates/inserts).
    /// </summary>
    protected virtual TEntity MapSingleToEntity(TDto dto)
    {
        if (dto == null)
        {
            Logger.LogWarning("Attempted to map null DTO of type {DtoType}", typeof(TDto).Name);
            throw new ArgumentNullException(nameof(dto));
        }

        try
        {
            var entity = dto.Adapt<TEntity>();
            Logger.LogDebug("Successfully mapped {DtoType} to {EntityType}", typeof(TDto).Name, typeof(TEntity).Name);
            return entity;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error mapping {DtoType} to {EntityType}", typeof(TDto).Name, typeof(TEntity).Name);
            throw;
        }
    }
}

