using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.DTOs;
using EHRPlatform.Services.Appointment.Application.Appointments.Responses;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Queries;

/// <summary>
/// Get provider appointment calendar - query for all appointments on a specific date.
/// </summary>
public record GetProviderAppointmentsQuery : IQuery<ProviderAppointmentCalendarDto>
{
    /// <summary>Provider ID to fetch appointments for.</summary>
    public Guid ProviderId { get; init; }

    /// <summary>Calendar date (appointments on this date).</summary>
    public DateTime CalendarDate { get; init; }

    /// <summary>Optional status filter (e.g., "Scheduled", "Confirmed", "Completed").</summary>
    public string? StatusFilter { get; init; }
}
