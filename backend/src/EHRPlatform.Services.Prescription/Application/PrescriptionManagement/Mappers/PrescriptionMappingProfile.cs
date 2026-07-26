using Mapster;
using EHRPlatform.Services.Prescription.Application.PrescriptionManagement.Responses;
using EHRPlatform.Services.Prescription.Domain.Entities;

namespace EHRPlatform.Services.Prescription.Application.PrescriptionManagement.Mappers;

/// <summary>
/// Mapster registration profile for Prescription entity mappings.
/// Handles conversion between domain models and DTOs.
/// Single Responsibility: Configure all Prescription-related type mappings.
/// Part of Application Layer (bridges Domain and Presentation).
/// </summary>
public class PrescriptionMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Prescription → PrescriptionResponseDto
        config.NewConfig<PrescriptionEntity, PrescriptionResponseDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.PatientId, src => src.PatientId)
            .Map(dest => dest.ProviderId, src => src.ProviderId)
            .Map(dest => dest.MedicationName, src => src.MedicationName)
            .Map(dest => dest.Strength, src => src.Strength)
            .Map(dest => dest.FormType, src => src.FormType)
            .Map(dest => dest.Dosage, src => src.Dosage)
            .Map(dest => dest.Frequency, src => src.Frequency)
            .Map(dest => dest.Quantity, src => src.Quantity)
            .Map(dest => dest.RefillsAllowed, src => src.RefillsAllowed)
            .Map(dest => dest.RefillsUsed, src => src.RefillsUsed)
            .Map(dest => dest.StartDate, src => src.StartDate)
            .Map(dest => dest.EndDate, src => src.EndDate)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.Indications, src => src.Indications)
            .Map(dest => dest.SpecialInstructions, src => src.SpecialInstructions)
            .Map(dest => dest.IsControlledSubstance, src => src.IsControlledSubstance)
            .Map(dest => dest.NDCCode, src => src.NDCCode)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt);

        // PrescriptionRefill → RefillDetailDto
        config.NewConfig<PrescriptionRefill, RefillDetailDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.RequestedAt, src => src.RequestedAt)
            .Map(dest => dest.ApprovedAt, src => src.ApprovedAt)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.PharmacyId, src => src.PharmacyId);
    }
}
