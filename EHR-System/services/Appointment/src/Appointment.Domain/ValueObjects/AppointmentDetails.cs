using EHRPlatform.Services.Appointment.Domain.Enums;

namespace EHRPlatform.Services.Appointment.Domain.ValueObjects;

/// <summary>
/// Value object representing appointment details (reason, notes, type).
/// Encapsulates clinical and contextual information about an appointment.
/// </summary>
public class AppointmentDetails : IEquatable<AppointmentDetails>
{
    /// <summary>
    /// Gets the appointment type (Office, Telehealth, Phone).
    /// </summary>
    public AppointmentType AppointmentType { get; }

    /// <summary>
    /// Gets the reason for visit.
    /// </summary>
    public string? ReasonForVisit { get; }

    /// <summary>
    /// Gets the appointment notes.
    /// </summary>
    public string? Notes { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AppointmentDetails"/> class.
    /// </summary>
    /// <param name="appointmentType">The appointment type.</param>
    /// <param name="reasonForVisit">The reason for visit.</param>
    /// <param name="notes">Additional notes.</param>
    /// <exception cref="ArgumentException">Thrown when details are invalid.</exception>
    public AppointmentDetails(
        AppointmentType appointmentType,
        string? reasonForVisit = null,
        string? notes = null)
    {
        if (string.IsNullOrWhiteSpace(reasonForVisit) && string.IsNullOrWhiteSpace(notes))
            throw new ArgumentException("Either reason for visit or notes must be provided");

        if (!string.IsNullOrEmpty(reasonForVisit) && reasonForVisit.Length > 500)
            throw new ArgumentException("Reason for visit cannot exceed 500 characters", nameof(reasonForVisit));

        if (!string.IsNullOrEmpty(notes) && notes.Length > 2000)
            throw new ArgumentException("Notes cannot exceed 2000 characters", nameof(notes));

        AppointmentType = appointmentType;
        ReasonForVisit = reasonForVisit?.Trim();
        Notes = notes?.Trim();
    }

    /// <summary>
    /// Determines whether this is a telehealth appointment.
    /// </summary>
    public bool IsTelehealth => AppointmentType == AppointmentType.Telehealth;

    /// <summary>
    /// Determines whether this is an in-office appointment.
    /// </summary>
    public bool IsOfficeVisit => AppointmentType == AppointmentType.Office;

    /// <summary>
    /// Determines whether this is a phone appointment.
    /// </summary>
    public bool IsPhoneAppointment => AppointmentType == AppointmentType.Phone;

    public bool Equals(AppointmentDetails? other)
    {
        return other != null &&
               AppointmentType == other.AppointmentType &&
               ReasonForVisit == other.ReasonForVisit &&
               Notes == other.Notes;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as AppointmentDetails);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(AppointmentType, ReasonForVisit, Notes);
    }

    public override string ToString()
    {
        return $"{AppointmentType} - {ReasonForVisit ?? "No reason specified"}";
    }
}
