using EHRPlatform.BuildingBlocks.SharedKernel.Entities;
using EHRPlatform.Services.Billing.Domain.Enums;

namespace EHRPlatform.Services.Billing.Domain.Entities;

/// <summary>
/// Insurance claim tracking — full EDI-ready claim record.
/// Tracks the lifecycle of a submitted insurance claim including
/// prior authorization, fraud scoring, and payer response.
/// </summary>
public class InsuranceClaim : BaseEntity
{
    public Guid InvoiceId { get; set; }

    // Insurance payer details
    public string InsuranceProvider { get; set; } = string.Empty;
    public string? PayerId { get; set; }               // Payer EDI ID (X12 loop 2010BB)
    public string? MemberId { get; set; }              // Patient member/subscriber ID
    public string? GroupNumber { get; set; }           // Insurance group number

    // Claim identifiers
    public string ClaimNumber { get; set; } = string.Empty;
    public string? PriorAuthorizationNumber { get; set; }  // PA# required for some procedures
    public string? Npi { get; set; }                   // Billing provider NPI (10-digit)

    // Lifecycle
    public ClaimStatus Status { get; set; } = ClaimStatus.Submitted;
    public DateTime SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public DateTime? DeniedAt { get; set; }

    // Financials
    public decimal Amount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public string? DenialReason { get; set; }
    public string? DenialCode { get; set; }            // CAS segment denial code (e.g., CO-97)

    // Fraud detection
    public decimal FraudScore { get; set; }            // 0–100; >80 = high risk → OnHold
    public string? FraudFlags { get; set; }            // JSON array of flag descriptions

    // Navigation
    public Invoice Invoice { get; set; } = null!;

    // Domain methods
    public void Approve(decimal approvedAmount)
    {
        Status = ClaimStatus.Approved;
        ApprovedAmount = approvedAmount;
        ApprovedAt = DateTime.UtcNow;
    }

    public void Deny(string reason, string? code = null)
    {
        Status = ClaimStatus.Denied;
        DenialReason = reason;
        DenialCode = code;
        DeniedAt = DateTime.UtcNow;
    }

    public void PlaceOnHold(string fraudFlag)
    {
        Status = ClaimStatus.OnHold;
        FraudFlags = fraudFlag;
    }
}


