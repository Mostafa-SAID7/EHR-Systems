using EHRPlatform.Common.Specifications;
using EHRPlatform.Services.Appointment.Features.Appointments.Domain;

namespace EHRPlatform.Services.Appointment.Domain.Specifications;

/// <summary>
/// Specification for retrieving provider availability slots.
/// Filters availability slots for a specific provider within a date range.
/// </summary>
public class ProviderAvailabilitySlotsSpecification : Specification<ProviderAvailability>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ProviderAvailabilitySlotsSpecification"/> class.
    /// </summary>
    /// <param name="providerId">The provider identifier.</param>
    /// <param name="startDate">The start date for the availability range.</param>
    /// <param name="endDate">The end date for the availability range.</param>
    public ProviderAvailabilitySlotsSpecification(
        Guid providerId,
        DateTime startDate,
        DateTime endDate)
    {
        AddCriteria(a => a.ProviderId == providerId &&
                        a.SlotStart >= startDate &&
                        a.SlotEnd <= endDate &&
                        a.IsActive);

        AddOrderBy(a => a.SlotStart);
    }
}
