namespace EHRPlatform.Services.Appointment.Application.Features.Appointments.Queries;

using MediatR;
using EHRPlatform.Services.Appointment.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for GetAppointmentQuery - Retrieves appointment details.
/// </summary>
public class GetAppointmentQueryHandler : IRequestHandler<GetAppointmentQuery, GetAppointmentResponse>
{
    private readonly IAppointmentDbContext _context;
    private readonly ILogger<GetAppointmentQueryHandler> _logger;

    public GetAppointmentQueryHandler(
        IAppointmentDbContext context,
        ILogger<GetAppointmentQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<GetAppointmentResponse> Handle(GetAppointmentQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting appointment: {AppointmentId}", request.AppointmentId);

        try
        {
            var appointment = await _context.Appointments
                .FirstOrDefaultAsync(a => a.Id == request.AppointmentId, cancellationToken);

            if (appointment == null)
            {
                return new GetAppointmentResponse
                {
                    Success = false,
                    Message = "Appointment not found"
                };
            }

            var appointmentDto = new AppointmentDto
            {
                Id = appointment.Id,
                PatientId = appointment.PatientId,
                ProviderId = appointment.ProviderId,
                AppointmentType = appointment.AppointmentType,
                ScheduledStart = appointment.ScheduledStart,
                ScheduledEnd = appointment.ScheduledEnd,
                Status = appointment.Status,
                ReasonForVisit = appointment.ReasonForVisit,
                DurationMinutes = appointment.GetDurationMinutes(),
                CreatedAt = appointment.CreatedAt
            };

            return new GetAppointmentResponse
            {
                Success = true,
                Appointment = appointmentDto
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting appointment");
            return new GetAppointmentResponse
            {
                Success = false,
                Message = "An error occurred while retrieving the appointment"
            };
        }
    }
}
