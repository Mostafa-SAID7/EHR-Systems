namespace EHRPlatform.Services.Billing.Domain.Entities;

/// <summary>
/// Insurance claim tracking claim submission and status.
/// Single Responsibility: Manage insurance claim lifecycle and approval tracking.
/// </summary>
public class InsuranceClaim
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public string InsuranceProvider { get; set; } = string.Empty;
    public string ClaimNumber { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string Status { get; set; } = "Submitted"; // Submitted, Approved, Denied, OnHold
    public decimal Amount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public string? DenialReason { get; set; }
    public string? FraudFlag { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public Invoice Invoice { get; set; } = null!;

    public void Approve(decimal approvedAmount)
    {
        Status = "Approved";
        ApprovedAmount = approvedAmount;
        ApprovedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deny(string reason, string? code = null)
    {
        Status = "Denied";
        DenialReason = reason;
        if (!string.IsNullOrEmpty(code))
            DenialReason = $"{code}: {reason}";
        UpdatedAt = DateTime.UtcNow;
    }

    public void PlaceOnHold(string fraudFlag)
    {
        Status = "OnHold";
        FraudFlag = fraudFlag;
        UpdatedAt = DateTime.UtcNow;
    }
}
