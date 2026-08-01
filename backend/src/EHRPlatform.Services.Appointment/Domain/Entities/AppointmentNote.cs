using EHRPlatform.BuildingBlocks.SharedKernel.Entities;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Domain;

/// <summary>
/// Appointment note/comment entity.
/// Allows providers and staff to add notes to appointments.
/// Supports privacy levels (private, shared with patient, internal only).
/// </summary>
public class AppointmentNote : AuditableEntity
{
    /// <summary>
    /// Gets or sets the appointment identifier this note belongs to.
    /// </summary>
    public Guid AppointmentId { get; set; }

    /// <summary>
    /// Gets or sets the user identifier who created the note.
    /// </summary>
    public Guid CreatedById { get; set; }

    /// <summary>
    /// Gets or sets the note content.
    /// </summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the privacy level (Private, SharedWithPatient, InternalOnly).
    /// </summary>
    public NotePrivacyLevel PrivacyLevel { get; set; } = NotePrivacyLevel.InternalOnly;

    /// <summary>
    /// Gets or sets the note category (Clinical, Administrative, Patient Communication, etc.)
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// Gets or sets whether the note is marked for follow-up.
    /// </summary>
    public bool IsFollowUpRequired { get; set; }

    /// <summary>
    /// Gets or sets the parent appointment.
    /// </summary>
    public Appointment Appointment { get; set; } = null!;

    /// <summary>
    /// Updates the note content.
    /// </summary>
    /// <param name="content">New note content.</param>
    public void UpdateContent(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Note content cannot be empty");

        Content = content;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Marks the note for follow-up.
    /// </summary>
    public void MarkForFollowUp()
    {
        IsFollowUpRequired = true;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Unmarks the note from follow-up.
    /// </summary>
    public void UnmarkFollowUp()
    {
        IsFollowUpRequired = false;
        UpdatedAt = DateTime.UtcNow;
    }
}

/// <summary>
/// Privacy level for appointment notes.
/// </summary>
public enum NotePrivacyLevel
{
    /// <summary>Private - visible only to provider who created it.</summary>
    Private = 1,

    /// <summary>Shared with patient - visible to patient and provider.</summary>
    SharedWithPatient = 2,

    /// <summary>Internal only - visible to clinical staff only.</summary>
    InternalOnly = 3
}


