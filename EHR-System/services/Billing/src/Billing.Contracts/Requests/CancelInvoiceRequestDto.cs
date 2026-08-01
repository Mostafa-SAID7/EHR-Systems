namespace EHRPlatform.Services.Billing.Contracts.Requests;

/// <summary>
/// Request DTO for cancelling invoice
/// </summary>
public class CancelInvoiceRequestDto
{
    /// <summary>Gets or sets cancellation reason.</summary>
    public string Reason { get; set; } = string.Empty;
}
