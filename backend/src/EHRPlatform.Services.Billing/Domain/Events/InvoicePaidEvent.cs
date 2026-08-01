using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Billing.Domain.Events;

/// <summary>
/// Invoice paid event.
/// Single Responsibility: Notify when invoice is fully paid.
/// </summary>
public class InvoicePaidEvent : IntegrationEvent
{
    public Guid InvoiceId { get; set; }
    public Guid PatientId { get; set; }
    public decimal Amount { get; set; }

    public InvoicePaidEvent(Guid id, Guid patientId, decimal amount)
    {
        InvoiceId = id;
        PatientId = patientId;
        Amount = amount;
    }
}

