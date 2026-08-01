using System;

namespace EHRPlatform.EventBus.Events;

/// <summary>
/// Published when an invoice is generated.
/// Consumed by: Notification (send invoice), Analytics, Audit.
/// Single responsibility: Invoice generation event.
/// </summary>
public class InvoiceGeneratedIntegrationEvent : IntegrationEvent
{
    public Guid InvoiceId { get; set; }
    public Guid PatientId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public DateTime DueDate { get; set; }
}

/// <summary>
/// Published when payment is processed.
/// Consumed by: Notification (send receipt), Analytics, Accounting.
/// Single responsibility: Payment processing event.
/// </summary>
public class PaymentProcessedIntegrationEvent : IntegrationEvent
{
    public Guid PaymentId { get; set; }
    public Guid InvoiceId { get; set; }
    public Guid PatientId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!;
    public string Status { get; set; } = null!;
}
