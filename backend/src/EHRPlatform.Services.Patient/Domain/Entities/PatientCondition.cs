using EHRPlatform.Common.Domain.Entities;

namespace EHRPlatform.Services.Patient.Domain.Entities;

/// <summary>
/// Patient medical condition.
/// </summary>
public class PatientCondition : BaseEntity
{
    public Guid PatientId { get; set; }
    public string Condition { get; set; } = string.Empty;
    public string ICD10Code { get; set; } = string.Empty;
    public DateTime? OnsetDate { get; set; }
    public DateTime? ResolvedDate { get; set; }
    public Patient Patient { get; set; } = null!;
}

