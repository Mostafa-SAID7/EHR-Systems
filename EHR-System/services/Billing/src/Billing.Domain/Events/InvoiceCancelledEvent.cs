namespace EHRPlatform.Services.Billing.Domain.Events;

/// <summary>
/// Published when invoice is cancelled.
/// Triggers cleanup and reversal of related claims.
/// </summary>
public record InvoiceCancelledEvent(
    Guid InvoiceId,
    Guid PatientId,
    string Reason)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}
