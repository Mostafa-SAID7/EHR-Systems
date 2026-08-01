using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Billing.Domain.Events;

/// <summary>
/// Invoice created event.
/// Single Responsibility: Notify when invoice is created.
/// </summary>
public class InvoiceCreatedEvent : IntegrationEvent
{
    public Guid InvoiceId { get; set; }
    public Guid PatientId { get; set; }
    public decimal Amount { get; set; }
    public string InvoiceNumber { get; set; }

    public InvoiceCreatedEvent(Guid id, Guid patientId, decimal amount, string number)
    {
        InvoiceId = id;
        PatientId = patientId;
        Amount = amount;
        InvoiceNumber = number;
    }
}

