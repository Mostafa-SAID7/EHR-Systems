using Mapster;
using EHRPlatform.Services.Prescription.Domain.Entities;
using EHRPlatform.Services.Prescription.Application.Prescriptions.Responses;

namespace EHRPlatform.Services.Prescription.Application.Prescriptions.Mappers;

/// <summary>
/// Mapster registration profile for Prescriptions feature.
/// Handles conversion between Prescription domain model and Prescription response DTOs.
/// Single Responsibility: Configure Prescription-related type mappings only.
/// </summary>
public class PrescriptionMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Prescription → PrescriptionResponse
        config.NewConfig<Prescription, PrescriptionResponse>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.PatientId, src => src.PatientId)
            .Map(dest => dest.ProviderId, src => src.ProviderId)
            .Map(dest => dest.MedicationName, src => src.MedicationName)
            .Map(dest => dest.Dosage, src => src.Dosage)
            .Map(dest => dest.Frequency, src => src.Frequency)
            .Map(dest => dest.Quantity, src => src.Quantity)
            .Map(dest => dest.Status, src => src.Status);
    }
}
