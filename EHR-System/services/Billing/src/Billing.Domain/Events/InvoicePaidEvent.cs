namespace EHRPlatform.Services.Billing.Domain.Events;

/// <summary>
/// Published when invoice is fully paid.
/// Triggers financial closing and revenue recognition.
/// </summary>
public record InvoicePaidEvent(
    Guid InvoiceId,
    Guid PatientId,
    decimal TotalAmount)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}
