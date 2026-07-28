using EHRPlatform.Common.Specifications;
using EHRPlatform.Services.Appointment.Features.Appointments.Domain;

namespace EHRPlatform.Services.Appointment.Domain.Specifications;

/// <summary>
/// Specification for retrieving available provider slots for booking.
/// Filters only slots with available capacity.
/// </summary>
public class AvailableSlotsForBookingSpecification : Specification<ProviderAvailability>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AvailableSlotsForBookingSpecification"/> class.
    /// </summary>
    /// <param name="providerId">The provider identifier.</param>
    /// <param name="startDate">The start date for available slots.</param>
    /// <param name="endDate">The end date for available slots.</param>
    public AvailableSlotsForBookingSpecification(
        Guid providerId,
        DateTime startDate,
        DateTime endDate)
    {
        AddCriteria(a => a.ProviderId == providerId &&
                        a.IsActive &&
                        a.SlotStart >= startDate &&
                        a.SlotEnd <= endDate &&
                        (a.MaxAppointmentsPerSlot == null || a.CurrentBookings < a.MaxAppointmentsPerSlot.Value));

        AddOrderBy(a => a.SlotStart);
    }
}
