using EHRPlatform.Common.DTOs;

namespace EHRPlatform.Services.Patient.Application.PatientManagement.Responses;

/// <summary>
/// Patient response DTO (basic info).
/// Single Responsibility: Represent patient data in API responses.
/// Includes slug support for URL-friendly patient identification.
/// </summary>
public class PatientResponseDto : StatusDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    
    /// <summary>
    /// Medical Record Number (unique). Used as MRN slug basis.
    /// </summary>
    public string MRN { get; set; } = string.Empty;
    
    /// <summary>
    /// Slug based on MRN for URL-friendly patient lookup.
    /// Example: mrn-{mrnSlug}
    /// </summary>
    public string? MRNSlug { get; set; }
    
    public string BloodType { get; set; } = string.Empty;
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}

