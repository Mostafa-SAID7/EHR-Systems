namespace EHRPlatform.Services.Patient.Domain.Entities;

/// <summary>
/// Patient aggregate root - Core patient record with medical history.
/// MRN: Medical Record Number (format: MRN-YYYY-XXXXXX, unique)
/// Elasticsearch indexed for full-text search
/// </summary>
public class Patient
{
    public Guid Id { get; set; }
    public string Mrn { get; set; } = string.Empty; // MRN-2025-000001 (UNIQUE)
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime DateOfBirth { get; set; }
    public string Gender { get; set; } = string.Empty; // M, F, Other, Prefer not to say
    public string? MiddleName { get; set; }
    
    // Address
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string State { get; set; } = string.Empty;
    public string ZipCode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;
    
    // Contact preferences
    public string PreferredContactMethod { get; set; } = "Email"; // Email, Phone, SMS
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? EmergencyContactRelationship { get; set; }
    
    // Medical info
    public string? BloodType { get; set; } // O+, O-, A+, A-, B+, B-, AB+, AB-
    public string Status { get; set; } = "Active"; // Active, Inactive, Archived
    public string PatientType { get; set; } = "Individual"; // Individual, Organization
    
    // Flags
    public bool VIP { get; set; }
    public bool DoNotContact { get; set; }
    public bool IsDeceased { get; set; }
    public DateTime? DeceasedDate { get; set; }
    
    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ArchivedAt { get; set; }
    public bool IsArchived { get; set; }

    // Relations
    public ICollection<PatientAllergy> Allergies { get; } = new List<PatientAllergy>();
    public ICollection<PatientCondition> Conditions { get; } = new List<PatientCondition>();
    public ICollection<PatientTag> Tags { get; } = new List<PatientTag>();

    private readonly List<object> _domainEvents = new();

    public string GetFullName() => $"{FirstName} {LastName}".Trim();

    public int GetAge() => DateTime.UtcNow.Year - DateOfBirth.Year;

    public void UpdateContactInfo(string email, string phone, string preferredMethod)
    {
        Email = email;
        Phone = phone;
        PreferredContactMethod = preferredMethod;
        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new PatientUpdatedEvent(Id, Mrn, GetFullName()));
    }

    public void UpdateAddress(string street, string city, string state, string zip, string country)
    {
        Street = street;
        City = city;
        State = state;
        ZipCode = zip;
        Country = country;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddAllergy(string allergyCode, string allergyName, string severity, string? reactionDescription)
    {
        var allergy = new PatientAllergy
        {
            Id = Guid.NewGuid(),
            PatientId = Id,
            AllergyCode = allergyCode,
            AllergyName = allergyName,
            Severity = severity,
            ReactionDescription = reactionDescription,
            OnsetDate = DateTime.UtcNow
        };
        Allergies.Add(allergy);
        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new PatientAllergyAddedEvent(Id, Mrn, allergyName, severity));
    }

    public void AddCondition(string conditionCode, string conditionName, DateTime? onsetDate = null)
    {
        var condition = new PatientCondition
        {
            Id = Guid.NewGuid(),
            PatientId = Id,
            ConditionCode = conditionCode,
            ConditionName = conditionName,
            OnsetDate = onsetDate ?? DateTime.UtcNow,
            Status = "Active"
        };
        Conditions.Add(condition);
        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new PatientConditionAddedEvent(Id, Mrn, conditionName, conditionCode));
    }

    public void ApplyTag(string tagName, string category, string color)
    {
        var tag = new PatientTag
        {
            Id = Guid.NewGuid(),
            PatientId = Id,
            TagName = tagName,
            Category = category,
            Color = color,
            ResourceType = "Patient",
            AppliedAt = DateTime.UtcNow
        };
        Tags.Add(tag);
        UpdatedAt = DateTime.UtcNow;
    }

    public void MarkAsDeceased(DateTime deceasedDate)
    {
        IsDeceased = true;
        DeceasedDate = deceasedDate;
        Status = "Inactive";
        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new PatientDeceasedEvent(Id, Mrn));
    }

    public void Archive()
    {
        IsArchived = true;
        ArchivedAt = DateTime.UtcNow;
        Status = "Archived";
        UpdatedAt = DateTime.UtcNow;
        RaiseEvent(new PatientArchivedEvent(Id, Mrn));
    }

    public void RaiseEvent(object @event) => _domainEvents.Add(@event);
    public IReadOnlyList<object> GetDomainEvents() => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
}

/// <summary>
/// PatientAllergy - Allergy record (value object)
/// </summary>
public class PatientAllergy
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string AllergyCode { get; set; } = string.Empty; // SNOMED CT code
    public string AllergyName { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty; // Mild, Moderate, Severe
    public string? ReactionDescription { get; set; }
    public DateTime OnsetDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Patient Patient { get; set; } = null!;
}

/// <summary>
/// PatientCondition - Active/historical conditions (ICD-10)
/// </summary>
public class PatientCondition
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string ConditionCode { get; set; } = string.Empty; // ICD-10 code
    public string ConditionName { get; set; } = string.Empty;
    public DateTime OnsetDate { get; set; }
    public DateTime? ResolutionDate { get; set; }
    public string Status { get; set; } = "Active"; // Active, Resolved, Chronic
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public Patient Patient { get; set; } = null!;
}

/// <summary>
/// PatientTag - Flexible tagging system (demographics, risk, specialty)
/// </summary>
public class PatientTag
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string TagName { get; set; } = string.Empty; // High Risk, VIP, Frequent Visitor, etc.
    public string Category { get; set; } = string.Empty; // demographics, risk, specialty, engagement
    public string Color { get; set; } = string.Empty; // For UI: red, yellow, green, blue
    public string ResourceType { get; set; } = "Patient";
    public DateTime AppliedAt { get; set; }

    public Patient Patient { get; set; } = null!;
}

// Domain Events
public record PatientCreatedEvent(Guid PatientId, string Mrn, string FirstName, string LastName, string Email)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record PatientUpdatedEvent(Guid PatientId, string Mrn, string FullName)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record PatientAllergyAddedEvent(Guid PatientId, string Mrn, string AllergyName, string Severity)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record PatientConditionAddedEvent(Guid PatientId, string Mrn, string ConditionName, string ConditionCode)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record PatientArchivedEvent(Guid PatientId, string Mrn)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record PatientDeceasedEvent(Guid PatientId, string Mrn)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}
