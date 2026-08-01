namespace EHRPlatform.Services.Billing.Domain.Events;

/// <summary>
/// Published when a new invoice is created.
/// Used by outbox pattern for reliable event publishing.
/// </summary>
public record InvoiceCreatedEvent(
    Guid InvoiceId,
    Guid PatientId,
    decimal TotalAmount,
    string InvoiceNumber)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}
