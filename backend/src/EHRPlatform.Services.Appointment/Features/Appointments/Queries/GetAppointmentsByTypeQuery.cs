using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.DTOs;
using EHRPlatform.Services.Appointment.Application.AppointmentManagement.Responses;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Queries;

/// <summary>
/// Get appointments filtered by AppointmentType - CACHED query.
/// Enables slug-based URL routing for appointment type-specific queries.
/// Example: GET /api/v1/appointments/type/telehealth
/// Cache key uses type slug to ensure consistency.
/// </summary>
public record GetAppointmentsByTypeQuery : ICachedQuery<PagedResult<AppointmentResponseDto>>
{
    /// <summary>
    /// Appointment type filter (e.g., "Office", "Telehealth", "Phone").
    /// </summary>
    public string AppointmentType { get; init; } = string.Empty;

    /// <summary>
    /// Optional patient filter.
    /// </summary>
    public Guid? PatientId { get; init; }

    /// <summary>
    /// Optional provider filter.
    /// </summary>
    public Guid? ProviderId { get; init; }

    /// <summary>
    /// Optional date range start.
    /// </summary>
    public DateTime? FromDate { get; init; }

    /// <summary>
    /// Optional date range end.
    /// </summary>
    public DateTime? ToDate { get; init; }

    /// <summary>
    /// Pagination page number.
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Pagination page size.
    /// </summary>
    public int PageSize { get; init; } = 20;

    public string CacheKey => $"appointments_type_{AppointmentType.ToLower()}_{PatientId}_{ProviderId}_{FromDate:O}_{ToDate:O}_{PageNumber}_{PageSize}";
    public int CacheDurationSeconds => 600; // 10 minutes
}

/// <summary>
/// Get appointment details filtered by AppointmentType - CACHED query.
/// Returns detailed information including reminders and computed fields.
/// </summary>
public record GetAppointmentDetailsByTypeQuery : ICachedQuery<PagedResult<AppointmentDetailedResponseDto>>
{
    /// <summary>
    /// Appointment type filter (e.g., "Office", "Telehealth", "Phone").
    /// </summary>
    public string AppointmentType { get; init; } = string.Empty;

    /// <summary>
    /// Optional patient filter.
    /// </summary>
    public Guid? PatientId { get; init; }

    /// <summary>
    /// Optional provider filter.
    /// </summary>
    public Guid? ProviderId { get; init; }

    /// <summary>
    /// Pagination page number.
    /// </summary>
    public int PageNumber { get; init; } = 1;

    /// <summary>
    /// Pagination page size.
    /// </summary>
    public int PageSize { get; init; } = 20;

    public string CacheKey => $"appointments_detail_type_{AppointmentType.ToLower()}_{PatientId}_{ProviderId}_{PageNumber}_{PageSize}";
    public int CacheDurationSeconds => 600; // 10 minutes
}
