using System;
using System.Collections.Generic;

namespace EHRPlatform.Services.Patient.Domain.Entities
{
    /// <summary>
    /// Service-Specific Patient Entity
    /// This entity belongs ONLY to the Patient Service.
    /// Other services cannot reference this directly.
    /// Inter-service communication uses PatientDto from EHRPlatform.Common.Shared.DTOs
    /// </summary>
    public class Patient
    {
        public Guid Id { get; set; }

        /// <summary>Unique Medical Record Number (MRN)</summary>
        public string MedicalRecordNumber { get; set; }

        /// <summary>Patient's first name</summary>
        public string FirstName { get; set; }

        /// <summary>Patient's last name</summary>
        public string LastName { get; set; }

        /// <summary>Patient's middle name (optional)</summary>
        public string MiddleName { get; set; }

        /// <summary>Patient's date of birth</summary>
        public DateTime DateOfBirth { get; set; }

        /// <summary>Patient's gender (e.g., "Male", "Female", "Other")</summary>
        public string Gender { get; set; }

        /// <summary>Patient's email address</summary>
        public string Email { get; set; }

        /// <summary>Patient's phone number</summary>
        public string PhoneNumber { get; set; }

        /// <summary>Current status (e.g., "Active", "Inactive", "Archived")</summary>
        public string Status { get; set; } = "Active";

        // Relationships to other Patient Service entities
        public ICollection<PatientAllergy> Allergies { get; set; } = new List<PatientAllergy>();
        public ICollection<PatientCondition> Conditions { get; set; } = new List<PatientCondition>();
        public ICollection<PatientContact> Contacts { get; set; } = new List<PatientContact>();
        public ICollection<PatientInsurance> InsuranceInformation { get; set; } = new List<PatientInsurance>();
        public ICollection<PatientEmergencyContact> EmergencyContacts { get; set; } = new List<PatientEmergencyContact>();
        public PatientMedicalHistory MedicalHistory { get; set; }

        /// <summary>Creation timestamp (UTC)</summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>Last modification timestamp (UTC)</summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>Soft delete timestamp (UTC) - null if not deleted</summary>
        public DateTime? DeletedAt { get; set; }
    }

    /// <summary>
    /// Patient's contact/address information
    /// </summary>
    public class PatientContact
    {
        public Guid Id { get; set; }

        public Guid PatientId { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        /// <summary>Is this the primary address?</summary>
        public bool IsPrimary { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public Patient Patient { get; set; }
    }

    /// <summary>
    /// Patient's known allergies
    /// </summary>
    public class PatientAllergy
    {
        public Guid Id { get; set; }

        public Guid PatientId { get; set; }

        /// <summary>Name of allergen (e.g., "Penicillin", "Peanuts")</summary>
        public string AllergenName { get; set; }

        /// <summary>Type of allergen (e.g., "Medication", "Food", "Environmental")</summary>
        public string AllergenType { get; set; }

        /// <summary>Severity level (e.g., "Mild", "Moderate", "Severe")</summary>
        public string Severity { get; set; }

        /// <summary>Description of allergic reaction</summary>
        public string Reaction { get; set; }

        /// <summary>When did the allergy start?</summary>
        public DateTime? OnsetDate { get; set; }

        /// <summary>When did the allergy resolve? (null if still active)</summary>
        public DateTime? ResolvedDate { get; set; }

        /// <summary>Is this allergy currently active?</summary>
        public bool IsCurrent { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public Patient Patient { get; set; }
    }

    /// <summary>
    /// Patient's known medical conditions/diagnoses
    /// </summary>
    public class PatientCondition
    {
        public Guid Id { get; set; }

        public Guid PatientId { get; set; }

        /// <summary>Name of the condition (e.g., "Type 2 Diabetes")</summary>
        public string ConditionName { get; set; }

        /// <summary>ICD-10 code for this condition</summary>
        public string ICD10Code { get; set; }

        /// <summary>When was this condition diagnosed?</summary>
        public DateTime? OnsetDate { get; set; }

        /// <summary>When was this condition resolved? (null if ongoing)</summary>
        public DateTime? ResolutionDate { get; set; }

        /// <summary>Current status of the condition</summary>
        public string Status { get; set; }

        /// <summary>Additional clinical notes</summary>
        public string Notes { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public Patient Patient { get; set; }
    }

    /// <summary>
    /// Patient's insurance information
    /// </summary>
    public class PatientInsurance
    {
        public Guid Id { get; set; }

        public Guid PatientId { get; set; }

        /// <summary>Name of insurance company</summary>
        public string InsuranceCompanyName { get; set; }

        /// <summary>Policy number</summary>
        public string PolicyNumber { get; set; }

        /// <summary>Group number (if applicable)</summary>
        public string GroupNumber { get; set; }

        /// <summary>Member ID</summary>
        public string MemberId { get; set; }

        /// <summary>When does this insurance coverage start?</summary>
        public DateTime EffectiveDate { get; set; }

        /// <summary>When does this insurance coverage end? (null if ongoing)</summary>
        public DateTime? TerminationDate { get; set; }

        /// <summary>Is this the primary insurance?</summary>
        public bool IsPrimary { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }

        // Navigation property
        public Patient Patient { get; set; }
    }

    /// <summary>
    /// Patient's emergency contact information
    /// </summary>
    public class PatientEmergencyContact
    {
        public Guid Id { get; set; }

        public Guid PatientId { get; set; }

        /// <summary>Name of emergency contact</summary>
        public string ContactName { get; set; }

        /// <summary>Relationship to patient (e.g., "Spouse", "Parent", "Sibling")</summary>
        public string Relationship { get; set; }

        /// <summary>Phone number</summary>
        public string PhoneNumber { get; set; }

        /// <summary>Email address</summary>
        public string Email { get; set; }

        /// <summary>Is this the primary emergency contact?</summary>
        public bool IsPrimary { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Patient Patient { get; set; }
    }

    /// <summary>
    /// Summary of patient's medical history
    /// One per patient (1-to-1 relationship)
    /// </summary>
    public class PatientMedicalHistory
    {
        public Guid Id { get; set; }

        /// <summary>Reference to Patient (unique - one history per patient)</summary>
        public Guid PatientId { get; set; }

        /// <summary>Blood type (e.g., "O+", "AB-")</summary>
        public string BloodType { get; set; }

        /// <summary>Height in cm</summary>
        public decimal? Height { get; set; }

        /// <summary>Weight in kg</summary>
        public decimal? Weight { get; set; }

        /// <summary>Summary of surgical procedures</summary>
        public string SurgicalHistory { get; set; }

        /// <summary>Summary of family medical history</summary>
        public string FamilyHistory { get; set; }

        /// <summary>Social history (smoking, alcohol, occupation, etc.)</summary>
        public string SocialHistory { get; set; }

        /// <summary>Last time this history was updated</summary>
        public DateTime LastUpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public Patient Patient { get; set; }
    }
}
