using Mapster;
using EHRPlatform.Services.Patient.Application.PatientManagement.Responses;

namespace EHRPlatform.Services.Patient.Application.PatientManagement.Mappers;

/// <summary>
/// Mapster registration profile for Patient entity mappings.
/// Single Responsibility: Configure all Patient-related type mappings.
/// </summary>
public class PatientMappingProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // Patient → PatientResponseDto
        config.NewConfig<PatientEntity, PatientResponseDto>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.FirstName, src => src.FirstName)
            .Map(dest => dest.LastName, src => src.LastName)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
            .Map(dest => dest.DateOfBirth, src => src.DateOfBirth)
            .Map(dest => dest.Gender, src => src.Gender)
            .Map(dest => dest.MRN, src => src.MRN)
            .Map(dest => dest.BloodType, src => src.BloodType)
            .Map(dest => dest.EmergencyContact, src => src.EmergencyContact)
            .Map(dest => dest.EmergencyPhone, src => src.EmergencyPhone)
            .Map(dest => dest.Status, src => src.Status)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.LastModifiedAt, src => src.UpdatedAt);
    }
}
