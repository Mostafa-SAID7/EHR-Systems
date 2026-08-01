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

/// <summary>
/// Published when FHIR resource is synced from external system.
/// Single responsibility: FHIR resource sync event.
/// </summary>
public class FhirResourceSyncedIntegrationEvent : IntegrationEvent
{
    public Guid SyncId { get; set; }
    public Guid PatientId { get; set; }
    public string ResourceType { get; set; } = null!;
    public string ExternalId { get; set; } = null!;
}
