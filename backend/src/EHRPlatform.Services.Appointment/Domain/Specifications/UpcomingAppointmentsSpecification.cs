using EHRPlatform.Common.Specifications;
using EHRPlatform.Services.Appointment.Domain.Enums;
using Appointment = EHRPlatform.Services.Appointment.Features.Appointments.Domain.Appointment;

namespace EHRPlatform.Services.Appointment.Domain.Specifications;

/// <summary>
/// Specification for retrieving upcoming appointments.
/// Filters appointments that are scheduled in the future.
/// </summary>
public class UpcomingAppointmentsSpecification : Specification<Appointment>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UpcomingAppointmentsSpecification"/> class.
    /// </summary>
    /// <param name="patientId">Optional patient identifier to filter by.</param>
    /// <param name="providerId">Optional provider identifier to filter by.</param>
    /// <param name="daysAhead">Number of days ahead to include (default: 30).</param>
    public UpcomingAppointmentsSpecification(
        Guid? patientId = null,
        Guid? providerId = null,
        int daysAhead = 30)
    {
        var now = DateTime.UtcNow;
        var endDate = now.AddDays(daysAhead);

        AddCriteria(a => a.ScheduledStart >= now &&
                        a.ScheduledStart <= endDate &&
                        (a.Status == AppointmentStatus.Scheduled || a.Status == AppointmentStatus.Confirmed));

        if (patientId.HasValue)
            AddCriteria(a => a.PatientId == patientId.Value);

        if (providerId.HasValue)
            AddCriteria(a => a.ProviderId == providerId.Value);

        AddOrderBy(a => a.ScheduledStart);
    }
}
