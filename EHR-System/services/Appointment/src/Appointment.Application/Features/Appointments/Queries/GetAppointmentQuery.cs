using EHRPlatform.BuildingBlocks.EventBus.Behaviors;
using EHRPlatform.BuildingBlocks.EventBus.CQRS;
using EHRPlatform.BuildingBlocks.Contracts.DTOs;
using EHRPlatform.Services.Appointment.Application.Appointments.Responses;

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


