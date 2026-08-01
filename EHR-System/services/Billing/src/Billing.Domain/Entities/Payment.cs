namespace EHRPlatform.Services.Billing.Domain.Entities;

/// <summary>
/// Payment record tracking money received for an invoice.
/// Single Responsibility: Record payment transactions with method and reference.
/// </summary>
public class Payment
{
    public Guid Id { get; set; }
    public Guid InvoiceId { get; set; }
    public decimal Amount { get; set; }
    public string Method { get; set; } = string.Empty; // CreditCard, Check, ACH, etc.
    public string Reference { get; set; } = string.Empty; // Transaction ID, Check #, etc.
    public DateTime ReceivedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    public Invoice Invoice { get; set; } = null!;
}
