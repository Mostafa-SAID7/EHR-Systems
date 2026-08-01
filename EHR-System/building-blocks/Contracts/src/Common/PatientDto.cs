using System;

namespace EHRPlatform.Contracts.Common;

/// <summary>
/// Patient data transfer object (commonly referenced across services).
/// </summary>
public class PatientDto : EHRPlatform.Contracts.Dto.BaseDto
{
    /// <summary>
    /// Medical record number (MRN) - unique identifier for patient.
    /// </summary>
    public string MedicalRecordNumber { get; set; } = null!;

    /// <summary>
    /// Patient first name.
    /// </summary>
    public string FirstName { get; set; } = null!;

    /// <summary>
    /// Patient last name.
    /// </summary>
    public string LastName { get; set; } = null!;

    /// <summary>
    /// Patient date of birth.
    /// </summary>
    public DateTime DateOfBirth { get; set; }

    /// <summary>
    /// Patient email address.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Patient phone number.
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Patient status (Active, Inactive, etc.).
    /// </summary>
    public string Status { get; set; } = "Active";

    /// <summary>
    /// Full name convenience property.
    /// </summary>
    public string FullName => $"{FirstName} {LastName}";

    /// <summary>
    /// Age convenience property.
    /// </summary>
    public int Age => (int)((DateTime.Now - DateOfBirth).TotalDays / 365.25);
}
