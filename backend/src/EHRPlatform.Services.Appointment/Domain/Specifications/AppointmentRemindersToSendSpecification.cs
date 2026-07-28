using EHRPlatform.Common.Specifications;
using EHRPlatform.Services.Appointment.Domain.Enums;
using EHRPlatform.Services.Appointment.Features.Appointments.Domain;

namespace EHRPlatform.Services.Appointment.Domain.Specifications;

/// <summary>
/// Specification for retrieving reminders that need to be sent.
/// Filters reminders that are scheduled but not yet sent.
/// </summary>
public class AppointmentRemindersToSendSpecification : Specification<AppointmentReminder>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppointmentRemindersToSendSpecification"/> class.
    /// </summary>
    public AppointmentRemindersToSendSpecification()
    {
        var now = DateTime.UtcNow;

        AddCriteria(r => r.Status == ReminderStatus.Scheduled &&
                        r.ReminderTime <= now &&
                        !r.IsSent);

        AddOrderBy(r => r.ReminderTime);
    }
}
