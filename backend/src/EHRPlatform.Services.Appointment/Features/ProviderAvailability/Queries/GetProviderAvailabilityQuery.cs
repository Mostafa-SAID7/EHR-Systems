using EHRPlatform.Common.CQRS;
using EHRPlatform.Services.Appointment.Application.ProviderAvailability.Responses;

namespace EHRPlatform.Services.Appointment.Features.ProviderAvailability.Queries;

/// <summary>
/// Get provider availability slots - query for available time slots for scheduling.
/// </summary>
public record GetProviderAvailabilityQuery : IQuery<ProviderAvailabilityListDto>
{
    /// <summary>Provider ID to fetch availability for.</summary>
    public Guid ProviderId { get; init; }

    /// <summary>Start date for availability range.</summary>
    public DateTime FromDate { get; init; }

    /// <summary>End date for availability range.</summary>
    public DateTime ToDate { get; init; }

    /// <summary>Optional appointment type filter (e.g., "Office", "Telehealth").</summary>
    public string? AppointmentType { get; init; }
}
