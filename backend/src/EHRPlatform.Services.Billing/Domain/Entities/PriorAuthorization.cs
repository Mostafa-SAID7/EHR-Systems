using EHRPlatform.Common.Domain.Entities;
using EHRPlatform.Services.Billing.Domain.Enums;

namespace EHRPlatform.Services.Billing.Domain.Entities;

/// <summary>
/// Prior authorization request entity.
/// Tracks the payer pre-approval required before rendering certain
/// procedures, imaging, or specialty medications. Without a valid
/// PriorAuthorizationNumber, the payer may deny the claim (CO-15).
///
/// Lifecycle: Requested → PendingClinicalReview → Approved/Denied → (Expired)
/// </summary>
public class PriorAuthorization : BaseEntity
{
    /// <summary>The claim this PA was requested for. Nullable — PA may be requested before claim creation.</summary>
    public Guid? ClaimId { get; set; }

    /// <summary>The clinical note/encounter driving the request.</summary>
    public Guid ClinicalNoteId { get; set; }

    /// <summary>Patient whose coverage is being verified.</summary>
    public Guid PatientId { get; set; }

    /// <summary>Insurance provider processing the PA request.</summary>
    public string InsuranceProvider { get; set; } = string.Empty;

    /// <summary>Patient's member ID with the payer.</summary>
    public string MemberId { get; set; } = string.Empty;

    /// <summary>CPT code for the procedure requiring authorization.</summary>
    public string ProcedureCode { get; set; } = string.Empty;

    /// <summary>Primary ICD-10 diagnosis code supporting medical necessity.</summary>
    public string DiagnosisCode { get; set; } = string.Empty;

    /// <summary>Clinical justification narrative submitted to payer.</summary>
    public string ClinicalJustification { get; set; } = string.Empty;

    // Lifecycle
    public PriorAuthStatus Status { get; set; } = PriorAuthStatus.Requested;
    public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    public DateTime? DecisionAt { get; set; }

    /// <summary>Payer-issued authorization number. Required on claim submission.</summary>
    public string? AuthorizationNumber { get; set; }

    /// <summary>Authorized service window start date.</summary>
    public DateTime? AuthorizedFromDate { get; set; }

    /// <summary>Authorized service window end date. After this date, Status = Expired.</summary>
    public DateTime? AuthorizedToDate { get; set; }

    /// <summary>Denial reason code (e.g., "Not Medically Necessary", "Duplicate Request").</summary>
    public string? DenialReason { get; set; }

    /// <summary>Navigation — linked claim (set after claim is created).</summary>
    public InsuranceClaim? Claim { get; set; }

    // Domain methods

    public void Approve(string authorizationNumber, DateTime fromDate, DateTime toDate)
    {
        if (Status != PriorAuthStatus.Requested && Status != PriorAuthStatus.PendingClinicalReview)
            throw new InvalidOperationException("Cannot approve a PA that is not pending.");

        Status = PriorAuthStatus.Approved;
        AuthorizationNumber = authorizationNumber;
        AuthorizedFromDate = fromDate;
        AuthorizedToDate = toDate;
        DecisionAt = DateTime.UtcNow;
    }

    public void Deny(string reason)
    {
        Status = PriorAuthStatus.Denied;
        DenialReason = reason;
        DecisionAt = DateTime.UtcNow;
    }

    public bool IsExpired() =>
        Status == PriorAuthStatus.Approved &&
        AuthorizedToDate.HasValue &&
        AuthorizedToDate.Value < DateTime.UtcNow;
}

