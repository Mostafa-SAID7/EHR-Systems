using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Billing.Domain.Events;

/// <summary>
/// Payment received event.
/// Single Responsibility: Notify when payment is received.
/// </summary>
public class PaymentReceivedEvent : IntegrationEvent
{
    public Guid InvoiceId { get; set; }
    public Guid PatientId { get; set; }
    public decimal Amount { get; set; }
    public string NewStatus { get; set; }

    public PaymentReceivedEvent(Guid id, Guid patientId, decimal amount, string status)
    {
        InvoiceId = id;
        PatientId = patientId;
        Amount = amount;
        NewStatus = status;
    }
}

