using EHRPlatform.Common.Domain.Specifications;
using Appointment = EHRPlatform.Services.Appointment.Features.Appointments.Domain.Appointment;

namespace EHRPlatform.Services.Appointment.Domain.Specifications;

/// <summary>
/// Specification for retrieving appointments by provider and date.
/// Filters appointments for a specific provider on a calendar date.
/// </summary>
public class AppointmentByProviderAndDateSpecification : Specification<Appointment>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppointmentByProviderAndDateSpecification"/> class.
    /// </summary>
    /// <param name="providerId">The provider identifier.</param>
    /// <param name="calendarDate">The calendar date.</param>
    /// <param name="statusFilter">Optional status filter.</param>
    public AppointmentByProviderAndDateSpecification(
        Guid providerId,
        DateTime calendarDate,
        string? statusFilter = null)
    {
        var dayStart = calendarDate.Date;
        var dayEnd = dayStart.AddDays(1);

        AddCriteria(a => a.ProviderId == providerId &&
                        a.ScheduledStart >= dayStart &&
                        a.ScheduledStart < dayEnd);

        if (!string.IsNullOrEmpty(statusFilter))
            AddCriteria(a => a.Status.ToString() == statusFilter);

        AddOrderBy(a => a.ScheduledStart);
    }
}

