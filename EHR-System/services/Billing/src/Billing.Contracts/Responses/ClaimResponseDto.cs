namespace EHRPlatform.Services.Billing.Contracts.Responses;

/// <summary>
/// Insurance claim response DTO.
/// </summary>
public class ClaimResponseDto
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public string InsuranceProvider { get; set; } = string.Empty;
    public string ClaimNumber { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; }
    public DateTime? ApprovedAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal? ApprovedAmount { get; set; }
    public string? DenialReason { get; set; }
    public string? FraudFlag { get; set; }
    public DateTime CreatedAt { get; set; }
}
