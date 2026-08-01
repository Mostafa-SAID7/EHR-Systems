using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Billing.Domain.Events;

/// <summary>
/// Invoice cancelled event.
/// Single Responsibility: Notify when invoice is cancelled.
/// </summary>
public class InvoiceCancelledEvent : IntegrationEvent
{
    public Guid InvoiceId { get; set; }
    public Guid PatientId { get; set; }
    public string Reason { get; set; }

    public InvoiceCancelledEvent(Guid id, Guid patientId, string reason)
    {
        InvoiceId = id;
        PatientId = patientId;
        Reason = reason;
    }
}

