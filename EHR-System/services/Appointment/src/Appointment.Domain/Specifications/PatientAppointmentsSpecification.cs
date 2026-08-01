using EHRPlatform.BuildingBlocks.SharedKernel.Specifications;
using Appointment = EHRPlatform.Services.Appointment.Features.Appointments.Domain.Appointment;

namespace EHRPlatform.Services.Appointment.Domain.Specifications;

/// <summary>
/// Specification for retrieving a patient's appointments.
/// Filters appointments for a specific patient with optional date range.
/// </summary>
public class PatientAppointmentsSpecification : Specification<Appointment>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PatientAppointmentsSpecification"/> class.
    /// </summary>
    /// <param name="patientId">The patient identifier.</param>
    /// <param name="startDate">Optional start date for filtering.</param>
    /// <param name="endDate">Optional end date for filtering.</param>
    public PatientAppointmentsSpecification(
        Guid patientId,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        AddCriteria(a => a.PatientId == patientId);

        if (startDate.HasValue)
            AddCriteria(a => a.ScheduledStart >= startDate.Value);

        if (endDate.HasValue)
            AddCriteria(a => a.ScheduledStart <= endDate.Value);

        AddOrderByDescending(a => a.ScheduledStart);
    }
}


