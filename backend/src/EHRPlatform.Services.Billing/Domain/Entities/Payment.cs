using EHRPlatform.Common.Domain.Entities;

namespace EHRPlatform.Services.Billing.Domain.Entities;

/// <summary>
/// Payment record.
/// Single Responsibility: Track payment received for invoice.
/// </summary>
public class Payment : BaseEntity
{
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public DateTime ReceivedAt { get; set; }
    public Invoice Invoice { get; set; } = null!;
}

