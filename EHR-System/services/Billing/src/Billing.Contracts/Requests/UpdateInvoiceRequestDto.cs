namespace EHRPlatform.Services.Billing.Contracts.Requests;

/// <summary>
/// Request DTO for updating invoice
/// </summary>
public class UpdateInvoiceRequestDto
{
    /// <summary>Gets or sets notes.</summary>
    public string? Notes { get; set; }

    /// <summary>Gets or sets invoice status.</summary>
    public string? Status { get; set; }

    /// <summary>Gets or sets paid amount.</summary>
    public decimal? PaidAmount { get; set; }
}
