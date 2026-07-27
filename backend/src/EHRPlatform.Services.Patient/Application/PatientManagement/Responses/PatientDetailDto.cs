using EHRPlatform.Common.DTOs;

namespace EHRPlatform.Services.Patient.Application.PatientManagement.Responses;

/// <summary>
/// Patient detail DTO with relationships.
/// Includes allergies, conditions, and calculated fields.
/// Single Responsibility: Represent enriched patient data for detailed views.
/// Supports slug-based URL identification via MRN.
/// </summary>
public class PatientDetailDto : StatusDto
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public int Age { get; set; }
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
    public List<AllergyDetailDto> Allergies { get; set; } = new();
    public List<ConditionDetailDto> Conditions { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime? LastModifiedAt { get; set; }
}

