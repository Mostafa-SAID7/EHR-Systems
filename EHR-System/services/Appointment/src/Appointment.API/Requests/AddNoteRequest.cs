namespace EHRPlatform.Services.Appointment.Controllers.Requests;

/// <summary>
/// Request model for adding appointment notes.
/// </summary>
public class AddNoteRequest
{
    /// <summary>Gets or sets the note content.</summary>
    public string Content { get; set; } = string.Empty;

    /// <summary>Gets or sets the user ID who created the note.</summary>
    public Guid CreatedById { get; set; }

    /// <summary>Gets or sets the privacy level (Private, SharedWithPatient, InternalOnly).</summary>
    public string? PrivacyLevel { get; set; } = "InternalOnly";

    /// <summary>Gets or sets the note category.</summary>
    public string? Category { get; set; }
}
