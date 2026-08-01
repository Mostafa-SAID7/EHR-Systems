namespace EHRPlatform.Services.Appointment.Application.Features.Appointments.Commands;

using MediatR;

/// <summary>
/// Command to confirm scheduled appointment.
/// </summary>
public class ConfirmAppointmentCommand : IRequest<ConfirmAppointmentResponse>
{
    public Guid AppointmentId { get; set; }
}

public class ConfirmAppointmentResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
}
