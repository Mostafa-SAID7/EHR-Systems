using EHRPlatform.BuildingBlocks.SharedKernel.Entities;
using EHRPlatform.BuildingBlocks.Common.Events;
using EHRPlatform.Services.Clinical.Domain.Events;

namespace EHRPlatform.Services.Clinical.Domain.Entities;

/// <summary>
/// Clinical note aggregate - SOAP format (Subjective, Objective, Assessment, Plan).
/// </summary>
public class ClinicalNote : AuditableEntity
{
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime EncounterDate { get; set; }
    public string EncounterType { get; set; } = string.Empty; // Office, Telehealth, Emergency, Hospital
    public string Status { get; set; } = "Draft"; // Draft, Finalized, Locked

    // SOAP components
    public string Subjective { get; set; } = string.Empty; // Patient complaint, symptoms
    public string Objective { get; set; } = string.Empty; // Physical exam, observations, lab results
    public string Assessment { get; set; } = string.Empty; // Diagnosis, impression
    public string Plan { get; set; } = string.Empty; // Treatment, medications, follow-up

    // Collections
    public ICollection<VitalSigns> VitalSigns { get; } = new List<VitalSigns>();
    public ICollection<ClinicalDiagnosis> Diagnoses { get; } = new List<ClinicalDiagnosis>();
    public ICollection<ClinicalProcedure> Procedures { get; } = new List<ClinicalProcedure>();

    private readonly List<IntegrationEvent> _domainEvents = new();

    public void AddDiagnosis(string diagnosisCode, string diagnosisText, string type)
    {
        var diagnosis = new ClinicalDiagnosis
        {
            Id = Guid.NewGuid(),
            ClinicalNoteId = Id,
            DiagnosisCode = diagnosisCode,
            DiagnosisText = diagnosisText,
            DiagnosisType = type // Principal, Secondary
        };
        Diagnoses.Add(diagnosis);

        RaiseEvent(new DiagnosisRecordedEvent(Id, PatientId, diagnosisCode, diagnosisText));
    }

    public void RecordVitals(decimal temperature, int systolic, int diastolic, int heartRate, int respiratoryRate, decimal? weight = null)
    {
        var vitals = new VitalSigns
        {
            Id = Guid.NewGuid(),
            ClinicalNoteId = Id,
            RecordedAt = DateTime.UtcNow,
            Temperature = temperature,
            SystolicBP = systolic,
            DiastolicBP = diastolic,
            HeartRate = heartRate,
            RespiratoryRate = respiratoryRate,
            Weight = weight
        };
        VitalSigns.Add(vitals);

        RaiseEvent(new VitalSignsRecordedEvent(Id, PatientId, systolic, diastolic, heartRate));
    }

    public void AddProcedure(string procedureName, string procedureCode, string result = "")
    {
        var procedure = new ClinicalProcedure
        {
            Id = Guid.NewGuid(),
            ClinicalNoteId = Id,
            ProcedureName = procedureName,
            ProcedureCode = procedureCode,
            Result = result,
            PerformedAt = DateTime.UtcNow
        };
        Procedures.Add(procedure);

        RaiseEvent(new ProcedurePerformedEvent(Id, PatientId, procedureName, procedureCode));
    }

    public void Finalize()
    {
        if (Status != "Draft")
            throw new InvalidOperationException("Only draft notes can be finalized");

        Status = "Finalized";
        RaiseEvent(new ClinicalNoteCompletedEvent(Id, PatientId, EncounterDate));
    }

    public void RaiseEvent(IntegrationEvent @event) => _domainEvents.Add(@event);
    public IReadOnlyList<IntegrationEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
}


