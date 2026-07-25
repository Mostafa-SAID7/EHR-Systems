using Mapster;
using EHRPlatform.Common.Mapping;
using EHRPlatform.Services.Patient.Domain.Entities;
using EHRPlatform.Services.Patient.Application.Patients.Responses;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Patient.Application.Patients.Mappers;

/// <summary>
/// Patient Mapper
/// Single Responsibility: Convert between Patient domain models and DTOs.
/// </summary>
public class PatientMapper : MappingServiceBase<PatientEntity, PatientResponse>
{
    public PatientMapper(ILogger<PatientMapper> logger) : base(logger)
    {
    }

    public PatientResponse MapToResponse(PatientEntity patient)
    {
        Logger.LogDebug("Mapping patient {PatientId} to response DTO", patient.Id);

        return new PatientResponse
        {
            Id = patient.Id,
            FirstName = patient.FirstName,
            LastName = patient.LastName,
            Email = patient.Email,
            PhoneNumber = patient.PhoneNumber,
            DateOfBirth = patient.DateOfBirth,
            Gender = patient.Gender,
            MRN = patient.MRN,
            BloodType = patient.BloodType,
            EmergencyContact = patient.EmergencyContact,
            EmergencyPhone = patient.EmergencyPhone,
            Status = patient.Status,
            Allergies = patient.Allergies
                .Select(a => new PatientAllergyDto
                {
                    Id = a.Id,
                    Allergen = a.Allergen,
                    Severity = a.Severity,
                    Notes = a.Notes
                })
                .ToList(),
            Conditions = patient.Conditions
                .Select(c => new PatientConditionDto
                {
                    Id = c.Id,
                    Condition = c.Condition,
                    ICD10Code = c.ICD10Code,
                    OnsetDate = c.OnsetDate
                })
                .ToList(),
            CreatedAt = patient.CreatedAt,
            UpdatedAt = patient.UpdatedAt
        };
    }

    public PatientListDto MapToListDto(
        ICollection<PatientEntity> patients,
        int total,
        int pageNumber,
        int pageSize)
    {
        Logger.LogDebug("Mapping {Count} patients to paginated list DTO", patients.Count);

        return new PatientListDto
        {
            Items = patients.Select(MapToResponse).ToList(),
            Total = total,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }
}
