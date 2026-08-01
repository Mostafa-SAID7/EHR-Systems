using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Billing.Domain.Events;

/// <summary>
/// Invoice submitted to insurance event.
/// Single Responsibility: Notify when invoice is submitted to insurance.
/// </summary>
public class InvoiceSubmittedEvent : IntegrationEvent
{
    public Guid InvoiceId { get; set; }
    public Guid PatientId { get; set; }
    public decimal Amount { get; set; }
    public string InsuranceProvider { get; set; }

    public InvoiceSubmittedEvent(Guid id, Guid patientId, decimal amount, string provider)
    {
        InvoiceId = id;
        PatientId = patientId;
        Amount = amount;
        InsuranceProvider = provider;
    }
}

