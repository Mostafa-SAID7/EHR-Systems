namespace EHRPlatform.Services.Appointment.Application.Appointments.Requests;

public class ScheduleAppointmentRequest
{
    public Guid PatientId { get; set; }
    public Guid ProviderId { get; set; }
    public DateTime ScheduledStart { get; set; }
    public DateTime ScheduledEnd { get; set; }
    public string? AppointmentType { get; set; }
}
