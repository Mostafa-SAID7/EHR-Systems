namespace EHRPlatform.Services.Billing.Domain.Events;

/// <summary>
/// Published when invoice is submitted to insurance.
/// Triggers claim tracking and insurance communication.
/// </summary>
public record InvoiceSubmittedEvent(
    Guid InvoiceId,
    Guid PatientId,
    decimal InsuranceResponsibility,
    string InsuranceProvider)
{
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
}
