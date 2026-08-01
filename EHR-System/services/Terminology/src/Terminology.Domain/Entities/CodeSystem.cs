namespace EHRPlatform.Services.Terminology.Domain.Entities;

/// <summary>
/// CodeSystem aggregate root - Medical code system (ICD-10, CPT, RxNorm, LOINC, etc.)
/// </summary>
public class CodeSystem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty; // ICD-10, CPT, RxNorm, LOINC
    public string Version { get; set; } = string.Empty; // 2024, 2023, etc.
    public string Description { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty; // http://hl7.org/fhir/sid/icd-10-cm
    public bool IsActive { get; set; } = true;
    public int TotalCodes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public ICollection<MedicalCode> Codes { get; } = new List<MedicalCode>();
    public ICollection<CodeMapping> Mappings { get; } = new List<CodeMapping>();

    private readonly List<object> _domainEvents = new();

    public void RaiseEvent(object @event) => _domainEvents.Add(@event);
    public IReadOnlyList<object> GetDomainEvents() => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
}

/// <summary>
/// MedicalCode - Individual code within a code system
/// Examples: "E11.9" for Type 2 diabetes (ICD-10), "99213" for office visit (CPT)
/// </summary>
public class MedicalCode
{
    public Guid Id { get; set; }
    public Guid CodeSystemId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Display { get; set; } = string.Empty;
    public string? Definition { get; set; }
    public string Category { get; set; } = string.Empty; // Diagnosis, Procedure, Medication, Lab, etc.
    public bool IsActive { get; set; } = true;
    public int UsageCount { get; set; } = 0;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Elasticsearch indexing - for full-text search
    public string? SearchableText { get; set; }

    public CodeSystem CodeSystem { get; set; } = null!;
    public ICollection<CodeMapping> SourceMappings { get; } = new List<CodeMapping>();
    public ICollection<CodeMapping> TargetMappings { get; } = new List<CodeMapping>();
}

/// <summary>
/// CodeMapping - Mapping between codes in different systems (e.g., ICD-10 to SNOMED CT)
/// </summary>
public class CodeMapping
{
    public Guid Id { get; set; }
    public Guid SourceCodeId { get; set; }
    public Guid TargetCodeId { get; set; }
    public string MappingType { get; set; } = string.Empty; // EXACT_MATCH, NARROWER, BROADER, RELATED
    public decimal Confidence { get; set; } // 0.0 to 1.0
    public bool IsApproved { get; set; }
    public string? ApprovedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public MedicalCode SourceCode { get; set; } = null!;
    public MedicalCode TargetCode { get; set; } = null!;
}

/// <summary>
/// CodeValidation - Validation rules and compliance checks
/// </summary>
public class CodeValidation
{
    public Guid Id { get; set; }
    public Guid CodeSystemId { get; set; }
    public string ValidationRule { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public CodeSystem CodeSystem { get; set; } = null!;
}

// Domain Events
public record CodeSystemUpdatedEvent(Guid CodeSystemId, string Name, int TotalCodes)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record CodeAddedEvent(Guid CodeSystemId, string Code, string Display)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}

public record CodeMappingCreatedEvent(Guid SourceCodeId, Guid TargetCodeId, string MappingType)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}
