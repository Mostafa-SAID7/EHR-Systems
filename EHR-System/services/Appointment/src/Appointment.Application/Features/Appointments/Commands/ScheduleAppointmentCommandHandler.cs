namespace EHRPlatform.Services.Appointment.Application.Features.Appointments.Commands;

using MediatR;
using EHRPlatform.Services.Appointment.Domain.Entities;
using EHRPlatform.Services.Appointment.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for ScheduleAppointmentCommand - Schedules appointment with conflict detection.
/// </summary>
public class ScheduleAppointmentCommandHandler : IRequestHandler<ScheduleAppointmentCommand, ScheduleAppointmentResponse>
{
    private readonly IAppointmentDbContext _context;
    private readonly ILogger<ScheduleAppointmentCommandHandler> _logger;

    public ScheduleAppointmentCommandHandler(
        IAppointmentDbContext context,
        ILogger<ScheduleAppointmentCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ScheduleAppointmentResponse> Handle(ScheduleAppointmentCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Scheduling appointment: Patient={PatientId}, Provider={ProviderId}, Start={ScheduledStart}",
            request.PatientId, request.ProviderId, request.ScheduledStart);

        try
        {
            // Check for provider conflicts
            var conflicts = await _context.Appointments
                .Where(a => a.ProviderId == request.ProviderId &&
                           a.Status != "Cancelled" &&
                           a.ScheduledStart < request.ScheduledEnd &&
                           a.ScheduledEnd > request.ScheduledStart)
                .FirstOrDefaultAsync(cancellationToken);

            if (conflicts != null)
            {
                _logger.LogWarning("Appointment conflict detected for provider {ProviderId}", request.ProviderId);
                return new ScheduleAppointmentResponse
                {
                    Success = false,
                    Message = "Provider has conflicting appointment"
                };
            }

            // Check for patient conflicts
            var patientConflicts = await _context.Appointments
                .Where(a => a.PatientId == request.PatientId &&
                           a.Status != "Cancelled" &&
                           a.ScheduledStart < request.ScheduledEnd &&
                           a.ScheduledEnd > request.ScheduledStart)
                .FirstOrDefaultAsync(cancellationToken);

            if (patientConflicts != null)
            {
                _logger.LogWarning("Appointment conflict detected for patient {PatientId}", request.PatientId);
                return new ScheduleAppointmentResponse
                {
                    Success = false,
                    Message = "Patient has conflicting appointment"
                };
            }

            // Create appointment
            var appointment = new Appointment
            {
                Id = Guid.NewGuid(),
                PatientId = request.PatientId,
                ProviderId = request.ProviderId,
                ScheduledStart = request.ScheduledStart,
                ScheduledEnd = request.ScheduledEnd,
                AppointmentType = request.AppointmentType,
                ReasonForVisit = request.ReasonForVisit,
                CreatedAt = DateTime.UtcNow
            };

            // Schedule reminders
            foreach (var reminder in request.Reminders)
            {
                appointment.ScheduleReminder(reminder.Method, reminder.MinutesBefore);
            }

            _context.Appointments.Add(appointment);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Appointment scheduled successfully: {AppointmentId}", appointment.Id);

            return new ScheduleAppointmentResponse
            {
                Success = true,
                AppointmentId = appointment.Id,
                Message = "Appointment scheduled successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling appointment");
            return new ScheduleAppointmentResponse
            {
                Success = false,
                Message = "An error occurred while scheduling the appointment"
            };
        }
    }
}
