namespace EHRPlatform.Services.Appointment.Application.Appointments.Responses;

/// <summary>
/// Provider appointment calendar DTO.
/// Contains all appointments for a provider on a specific date.
/// </summary>
public class ProviderAppointmentCalendarDto
{
    /// <summary>Gets or sets the provider identifier.</summary>
    public Guid ProviderId { get; set; }

    /// <summary>Gets or sets the calendar date.</summary>
    public DateTime Date { get; set; }

    /// <summary>Gets or sets the list of appointment slots for the date.</summary>
    public List<AppointmentSlotDto> Slots { get; set; } = new();
}
