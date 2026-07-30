using Mapster;
using EHRPlatform.Common.Application.Mapping;
using EHRPlatform.Common.Shared.DTOs;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Patient.Application.PatientManagement.Responses;

namespace EHRPlatform.Services.Patient.Application.PatientManagement.Mappers;

/// <summary>
/// Patient Mapper.
/// Single Responsibility: Convert between Patient domain model and DTOs.
/// </summary>
public class PatientMapper : MappingServiceBase<PatientEntity, PatientResponseDto>
{
    public PatientMapper(ILogger<PatientMapper> logger) : base(logger)
    {
    }

    /// <summary>
    /// Map single patient to response DTO.
    /// </summary>
    public PatientResponseDto MapToResponseDto(PatientEntity patient)
    {
        return MapSingleToDto(patient);
    }

    /// <summary>
    /// Map collection of patients to response DTO list.
    /// </summary>
    public List<PatientResponseDto> MapToResponseDtoList(ICollection<PatientEntity> patients)
    {
        Logger.LogDebug("Mapping {Count} patients to response DTO list", patients.Count);
        return patients.Adapt<List<PatientResponseDto>>();
    }

    /// <summary>
    /// Map patients to paged result.
    /// </summary>
    public PagedResult<PatientResponseDto> MapToPagedResult(
        ICollection<PatientEntity> patients,
        int total,
        int pageNumber,
        int pageSize)
    {
        Logger.LogDebug("Mapping {Count} patients to paged result", patients.Count);

        return PagedResult<PatientResponseDto>.Create(
            patients.Adapt<List<PatientResponseDto>>(),
            total,
            pageNumber,
            pageSize);
    }
}

