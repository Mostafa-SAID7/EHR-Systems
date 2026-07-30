using EHRPlatform.Common.Domain.Entities;

namespace EHRPlatform.Services.Billing.Domain.Entities;

/// <summary>
/// Invoice line item (charge/service).
/// Single Responsibility: Represent individual service charge on invoice.
/// </summary>
public class LineItem : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string CPTCode { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Amount { get; set; }
    public Invoice Invoice { get; set; } = null!;
}

