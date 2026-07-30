using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Services.Appointment.Application.Appointments.Responses;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Queries;

/// <summary>
/// Get pending appointment reminders query.
/// Retrieves all reminders scheduled to be sent now or in the past.
/// </summary>
public record GetPendingRemindersQuery : IQuery<IEnumerable<AppointmentReminderDto>>
{
}

