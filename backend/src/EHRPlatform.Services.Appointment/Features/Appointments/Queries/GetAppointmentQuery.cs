using EHRPlatform.Common.Behaviors;
using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.DTOs;
using EHRPlatform.Services.Appointment.Application.AppointmentManagement.Responses;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Queries;

/// <summary>
/// Get appointment by ID - CACHED query.
/// </summary>
public record GetAppointmentQuery : IQuery<AppointmentResponseDto>, ICachedQuery
{
    public Guid AppointmentId { get; init; }

    public string CacheKey => $"appointment_{AppointmentId}";
    public TimeSpan? Duration => TimeSpan.FromSeconds(600);
}

/// <summary>
/// Get patient appointments (paginated, optional date range filter).
/// </summary>
public record GetPatientAppointmentsQuery : IQuery<PagedResult<AppointmentResponseDto>>, ICachedQuery
{
    public Guid PatientId { get; init; }
    public DateTime? FromDate { get; init; }
    public DateTime? ToDate { get; init; }
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;

    public string CacheKey => $"appointments_patient_{PatientId}_{FromDate:yyyyMMdd}_{ToDate:yyyyMMdd}_{PageNumber}_{PageSize}";
    public TimeSpan? Duration => TimeSpan.FromSeconds(600);
}

/// <summary>
/// Get provider appointments (calendar view).
/// </summary>
public record GetProviderAppointmentsQuery : IQuery<ProviderAppointmentCalendarDto>, ICachedQuery
{
    public Guid ProviderId { get; init; }
    public DateTime Date { get; init; }

    public string CacheKey => $"appointments_provider_{ProviderId}_{Date:yyyyMMdd}";
    public TimeSpan? Duration => TimeSpan.FromSeconds(300);
}

/// <summary>
/// Get provider availability slots.
/// </summary>
public record GetProviderAvailabilityQuery : IQuery<ProviderAvailabilityListDto>, ICachedQuery
{
    public Guid ProviderId { get; init; }
    public DateTime FromDate { get; init; }
    public DateTime ToDate { get; init; }

    public string CacheKey => $"availability_{ProviderId}_{FromDate:yyyyMMdd}_{ToDate:yyyyMMdd}";
    public TimeSpan? Duration => TimeSpan.FromSeconds(300);
}
