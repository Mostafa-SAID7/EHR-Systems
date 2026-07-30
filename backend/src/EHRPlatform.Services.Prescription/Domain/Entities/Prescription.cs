using EHRPlatform.Common.Domain.Entities;
using EHRPlatform.Common.Events;
using EHRPlatform.Services.Prescription.Domain.Events;

namespace EHRPlatform.Services.Prescription.Domain.Entities;

/// <summary>
/// Prescription aggregate root.
/// Manages medication orders with dosage, refills, pharmacy coordination.
/// </summary>
public class Prescription : AuditableEntity
{
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string Strength { get; set; } = string.Empty; // e.g., "500mg"
    public string FormType { get; set; } = string.Empty; // Tablet, Capsule, Liquid, Injection
    public string Dosage { get; set; } = string.Empty; // e.g., "1 tablet"
    public string Frequency { get; set; } = string.Empty; // e.g., "twice daily"
    public int Quantity { get; set; }
    public int RefillsAllowed { get; set; }
    public int RefillsUsed { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public string Status { get; set; } = "Active"; // Active, Inactive, Suspended, Discontinued
    public string? Indications { get; set; } // Why prescribed
    public string? SpecialInstructions { get; set; }
    public string? PharmacyNotes { get; set; }
    public bool IsControlledSubstance { get; set; }
    public string? NDCCode { get; set; } // National Drug Code

    // Collections
    public ICollection<PrescriptionRefill> Refills { get; } = new List<PrescriptionRefill>();

    private readonly List<IntegrationEvent> _domainEvents = new();

    public bool CanRefill() => Status == "Active" && RefillsUsed < RefillsAllowed && (EndDate == null || EndDate > DateTime.UtcNow);

    public void RequestRefill(string pharmacyId = "")
    {
        if (!CanRefill())
            throw new InvalidOperationException("Prescription cannot be refilled");

        var refill = new PrescriptionRefill
        {
            Id = Guid.NewGuid(),
            PrescriptionId = Id,
            RequestedAt = DateTime.UtcNow,
            Status = "Pending",
            PharmacyId = pharmacyId
        };
        Refills.Add(refill);

        RaiseEvent(new PrescriptionRefillRequestedEvent(Id, PatientId, ProviderId, MedicationName));
    }

    public void ApproveRefill(Guid refillId)
    {
        var refill = Refills.FirstOrDefault(r => r.Id == refillId);
        if (refill == null)
            throw new InvalidOperationException("Refill not found");

        refill.Status = "Approved";
        refill.ApprovedAt = DateTime.UtcNow;
        RefillsUsed++;

        RaiseEvent(new PrescriptionRefillApprovedEvent(Id, PatientId, MedicationName));
    }

    public void Suspend(string reason = "")
    {
        if (Status == "Discontinued")
            throw new InvalidOperationException("Cannot suspend discontinued prescription");

        Status = "Suspended";
        RaiseEvent(new PrescriptionSuspendedEvent(Id, PatientId, MedicationName, reason));
    }

    public void Resume()
    {
        if (Status != "Suspended")
            throw new InvalidOperationException("Only suspended prescriptions can be resumed");

        Status = "Active";
        RaiseEvent(new PrescriptionResumedEvent(Id, PatientId, MedicationName));
    }

    public void Discontinue(string reason = "")
    {
        if (Status == "Discontinued")
            throw new InvalidOperationException("Prescription already discontinued");

        Status = "Discontinued";
        EndDate = DateTime.UtcNow;
        RaiseEvent(new PrescriptionDiscontinuedEvent(Id, PatientId, MedicationName, reason));
    }

    public void RaiseEvent(IntegrationEvent @event) => _domainEvents.Add(@event);
    public IReadOnlyList<IntegrationEvent> GetDomainEvents() => _domainEvents.AsReadOnly();
    public void ClearDomainEvents() => _domainEvents.Clear();
}

