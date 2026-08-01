namespace EHRPlatform.Services.Billing.Domain.Events;

/// <summary>
/// Published when a payment is recorded for an invoice.
/// Triggers reconciliation and accounting entries.
/// </summary>
public record PaymentReceivedEvent(
    Guid InvoiceId,
    Guid PatientId,
    decimal Amount,
    string NewInvoiceStatus)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}
