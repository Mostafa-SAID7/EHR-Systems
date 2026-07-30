using EHRPlatform.Common.Domain.Entities;
using EHRPlatform.Common.Events;
using EHRPlatform.Services.Patient.Domain.Events;

namespace EHRPlatform.Services.Patient.Domain.Entities;

/// <summary>
/// Patient aggregate root.
/// Full CQRS + domain events + audit trail.
/// </summary>
public class Patient : AuditableEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty;
    public string MRN { get; set; } = string.Empty; // Medical Record Number - unique
    public string BloodType { get; set; } = string.Empty;
    public string? EmergencyContact { get; set; }
    public string? EmergencyPhone { get; set; }
    public string Status { get; set; } = "Active"; // Active, Inactive, Transferred

    // Collections
    public ICollection<PatientAllergy> Allergies { get; } = new List<PatientAllergy>();
    public ICollection<PatientCondition> Conditions { get; } = new List<PatientCondition>();

    // Domain events
    private readonly List<IntegrationEvent> _domainEvents = new();

    public void AddAllergy(string allergen, string severity, string notes = "")
    {
        var allergy = new PatientAllergy
        {
            Id = Guid.NewGuid(),
            PatientId = Id,
            Allergen = allergen,
            Severity = severity,
            Notes = notes
        };
        Allergies.Add(allergy);

        RaiseEvent(new PatientAllergyAddedEvent(Id, allergen, severity));
    }

    public void AddCondition(string condition, string icd10Code, DateTime? onsetDate = null)
    {
        var cond = new PatientCondition
        {
            Id = Guid.NewGuid(),
            PatientId = Id,
            Condition = condition,
            ICD10Code = icd10Code,
            OnsetDate = onsetDate
        };
        Conditions.Add(cond);

        RaiseEvent(new PatientConditionAddedEvent(Id, condition, icd10Code));
    }

    public void RaiseEvent(IntegrationEvent @event) => _domainEvents.Add(@event);
    public IReadOnlyList<IntegrationEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
}

