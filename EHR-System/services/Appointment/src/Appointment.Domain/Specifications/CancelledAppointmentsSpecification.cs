using EHRPlatform.BuildingBlocks.SharedKernel.Specifications;
using EHRPlatform.Services.Appointment.Domain.Enums;
using Appointment = EHRPlatform.Services.Appointment.Features.Appointments.Domain.Appointment;

namespace EHRPlatform.Services.Appointment.Domain.Specifications;

/// <summary>
/// Specification for retrieving cancelled appointments.
/// Filters appointments that have been cancelled.
/// </summary>
public class CancelledAppointmentsSpecification : Specification<Appointment>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CancelledAppointmentsSpecification"/> class.
    /// </summary>
    /// <param name="patientId">Optional patient identifier to filter by.</param>
    /// <param name="providerId">Optional provider identifier to filter by.</param>
    /// <param name="reason">Optional cancellation reason to filter by.</param>
    public CancelledAppointmentsSpecification(
        Guid? patientId = null,
        Guid? providerId = null,
        CancellationReason? reason = null)
    {
        AddCriteria(a => a.Status == AppointmentStatus.Cancelled);

        if (patientId.HasValue)
            AddCriteria(a => a.PatientId == patientId.Value);

        if (providerId.HasValue)
            AddCriteria(a => a.ProviderId == providerId.Value);

        if (reason.HasValue)
            AddCriteria(a => a.CancellationReason == reason.Value);

        AddOrderByDescending(a => a.CancelledAt);
    }
}


