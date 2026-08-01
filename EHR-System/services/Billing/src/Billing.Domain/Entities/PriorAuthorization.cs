namespace EHRPlatform.Services.Billing.Domain.Entities;

/// <summary>
/// Prior authorization request for insurance coverage.
/// Single Responsibility: Track authorization requests and approvals.
/// </summary>
public class PriorAuthorization
{
    public Guid Id { get; set; }
    public Guid PatientId { get; set; }
    public string InsuranceProvider { get; set; } = string.Empty;
    public string ServiceCode { get; set; } = string.Empty;
    public string ProcedureName { get; set; } = string.Empty;
    public DateTime RequestedDate { get; set; }
    public string Status { get; set; } = "Pending"; // Pending, Approved, Denied, Expired
    public string? AuthorizationNumber { get; set; }
    public DateTime? AuthorizationFromDate { get; set; }
    public DateTime? AuthorizationToDate { get; set; }
    public int? AuthorizedUnits { get; set; }
    public decimal? AuthorizedAmount { get; set; }
    public string? DenialReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public void Approve(string authorizationNumber, DateTime fromDate, DateTime toDate)
    {
        Status = "Approved";
        AuthorizationNumber = authorizationNumber;
        AuthorizationFromDate = fromDate;
        AuthorizationToDate = toDate;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deny(string reason)
    {
        Status = "Denied";
        DenialReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    public bool IsExpired() =>
        Status == "Approved" && DateTime.UtcNow > AuthorizationToDate;
}
