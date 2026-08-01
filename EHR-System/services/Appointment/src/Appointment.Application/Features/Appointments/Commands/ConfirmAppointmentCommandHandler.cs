namespace EHRPlatform.Services.Appointment.Application.Features.Appointments.Commands;

using MediatR;
using EHRPlatform.Services.Appointment.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for ConfirmAppointmentCommand - Confirms scheduled appointment.
/// </summary>
public class ConfirmAppointmentCommandHandler : IRequestHandler<ConfirmAppointmentCommand, ConfirmAppointmentResponse>
{
    private readonly IAppointmentDbContext _context;
    private readonly ILogger<ConfirmAppointmentCommandHandler> _logger;

    public ConfirmAppointmentCommandHandler(
        IAppointmentDbContext context,
        ILogger<ConfirmAppointmentCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ConfirmAppointmentResponse> Handle(ConfirmAppointmentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Confirming appointment: {AppointmentId}", request.AppointmentId);

        try
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);

            if (appointment == null)
            {
                return new ConfirmAppointmentResponse
                {
                    Success = false,
                    Message = "Appointment not found"
                };
            }

            appointment.Confirm();
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Appointment confirmed: {AppointmentId}", request.AppointmentId);

            return new ConfirmAppointmentResponse
            {
                Success = true,
                Message = "Appointment confirmed"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error confirming appointment");
            return new ConfirmAppointmentResponse
            {
                Success = false,
                Message = "An error occurred while confirming the appointment"
            };
        }
    }
}
