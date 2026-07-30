using EHRPlatform.Common.Domain.Specifications;
using EHRPlatform.Services.Appointment.Domain.Enums;
using Appointment = EHRPlatform.Services.Appointment.Features.Appointments.Domain.Appointment;

namespace EHRPlatform.Services.Appointment.Domain.Specifications;

/// <summary>
/// Specification for retrieving appointments by type.
/// Filters appointments by appointment type (Office, Telehealth, Phone) with optional patient/provider filters.
/// </summary>
public class AppointmentByTypeSpecification : Specification<Appointment>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AppointmentByTypeSpecification"/> class.
    /// </summary>
    /// <param name="appointmentType">The appointment type to filter.</param>
    /// <param name="patientId">Optional patient identifier filter.</param>
    /// <param name="providerId">Optional provider identifier filter.</param>
    public AppointmentByTypeSpecification(
        AppointmentType appointmentType,
        Guid? patientId = null,
        Guid? providerId = null)
    {
        AddCriteria(a => a.AppointmentType == appointmentType);

        if (patientId.HasValue)
            AddCriteria(a => a.PatientId == patientId.Value);

        if (providerId.HasValue)
            AddCriteria(a => a.ProviderId == providerId.Value);

        AddOrderByDescending(a => a.ScheduledStart);
    }
}

