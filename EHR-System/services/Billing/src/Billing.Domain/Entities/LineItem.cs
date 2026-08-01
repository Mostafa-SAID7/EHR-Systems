namespace EHRPlatform.Services.Billing.Domain.Entities;

/// <summary>
/// Invoice line item representing a single service charge.
/// Single Responsibility: Represent individual service charge with CPT code and pricing.
/// </summary>
public class LineItem
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string CPTCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
    public DateTime CreatedAt { get; set; }

    public Invoice Invoice { get; set; } = null!;
}
