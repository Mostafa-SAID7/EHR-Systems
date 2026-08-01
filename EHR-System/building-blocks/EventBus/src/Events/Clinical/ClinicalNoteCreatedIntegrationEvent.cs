using System;

namespace EHRPlatform.EventBus.Events;

/// <summary>
/// Published when a clinical note is created.
/// Consumed by: Audit, FileStorage (link documents), Analytics.
/// Single responsibility: Clinical note creation event.
/// </summary>
public class ClinicalNoteCreatedIntegrationEvent : IntegrationEvent
{
    public Guid NoteId { get; set; }
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public string NoteType { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}
