using Mapster;
using EHRPlatform.Common.Application.Mapping;
using EHRPlatform.Common.Shared.DTOs;
using EHRPlatform.Services.Prescription.Application.PrescriptionManagement.Responses;
using PrescriptionRefillEntity = EHRPlatform.Services.Prescription.Domain.Entities.PrescriptionRefill;

namespace EHRPlatform.Services.Prescription.Application.PrescriptionManagement.Mappers;

/// <summary>
/// Prescription Mapper
/// Single Responsibility: Convert between Prescription domain models and DTOs.
/// Handles all Prescription-related mappings with optional post-processing.
/// Part of Application Layer (bridges Domain and Presentation).
/// </summary>
public class PrescriptionMapper : MappingServiceBase<PrescriptionEntity, PrescriptionResponseDto>
{
    public PrescriptionMapper(ILogger<PrescriptionMapper> logger) : base(logger)
    {
    }

    /// <summary>
    /// Map single prescription to response DTO.
    /// </summary>
    public PrescriptionResponseDto MapToResponseDto(PrescriptionEntity prescription)
    {
        return MapSingleToDto(prescription);
    }

    /// <summary>
    /// Map prescription to detailed DTO with refills.
    /// </summary>
    public PrescriptionDetailedDto MapToDetailedDto(PrescriptionEntity prescription)
    {
        Logger.LogDebug("Mapping prescription {PrescriptionId} to detailed DTO", prescription.Id);

        return new PrescriptionDetailedDto
        {
            Id = prescription.Id,
            PatientId = prescription.PatientId,
            ProviderId = prescription.ProviderId,
            MedicationName = prescription.MedicationName,
            Strength = prescription.Strength,
            FormType = prescription.FormType,
            Dosage = prescription.Dosage,
            Frequency = prescription.Frequency,
            Quantity = prescription.Quantity,
            RefillsAllowed = prescription.RefillsAllowed,
            RefillsUsed = prescription.RefillsUsed,
            StartDate = prescription.StartDate,
            EndDate = prescription.EndDate,
            Status = prescription.Status,
            Indications = prescription.Indications,
            SpecialInstructions = prescription.SpecialInstructions,
            IsControlledSubstance = prescription.IsControlledSubstance,
            NDCCode = prescription.NDCCode,
            Refills = prescription.Refills.Adapt<List<RefillDetailDto>>(),
            CreatedAt = prescription.CreatedAt,
            LastModifiedAt = prescription.UpdatedAt
        };
    }

    /// <summary>
    /// Map collection of prescriptions to paged result.
    /// </summary>
    public PagedResult<PrescriptionResponseDto> MapToPagedResult(
        ICollection<PrescriptionEntity> prescriptions,
        int total,
        int pageNumber,
        int pageSize)
    {
        Logger.LogDebug("Mapping {Count} prescriptions to paged result", prescriptions.Count);

        return PagedResult<PrescriptionResponseDto>.Create(
            prescriptions.Adapt<List<PrescriptionResponseDto>>(),
            total,
            pageNumber,
            pageSize);
    }

    /// <summary>
    /// Map collection of prescriptions to response DTO list.
    /// </summary>
    public List<PrescriptionResponseDto> MapToResponseDtoList(ICollection<PrescriptionEntity> prescriptions)
    {
        Logger.LogDebug("Mapping {Count} prescriptions to response DTO list", prescriptions.Count);
        return prescriptions.Adapt<List<PrescriptionResponseDto>>();
    }
}

