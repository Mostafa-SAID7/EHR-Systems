using System;

namespace EHRPlatform.EventBus.Events;

/// <summary>
/// Published when HL7 message is received and processed.
/// Single responsibility: HL7 message reception event.
/// </summary>
public class HL7MessageReceivedIntegrationEvent : IntegrationEvent
{
    public Guid MessageId { get; set; }
    public Guid PatientId { get; set; }
    public string ExternalSystem { get; set; } = null!;
    public DateTime ReceivedAt { get; set; }
}
