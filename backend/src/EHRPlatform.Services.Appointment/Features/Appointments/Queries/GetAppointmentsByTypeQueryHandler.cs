using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.DTOs;
using EHRPlatform.Common.Slugs;
using EHRPlatform.Services.Appointment.Application.AppointmentManagement.Responses;
using Mapster;
using ApptEntity = EHRPlatform.Services.Appointment.Features.Appointments.Domain.Appointment;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Queries;

/// <summary>
/// Get appointments by AppointmentType query handler.
/// Filters appointments by type (Office, Telehealth, Phone) with optional patient/provider filters.
/// Automatically cached by CachingBehavior.
/// Generates AppointmentTypeSlug for response.
/// </summary>
public class GetAppointmentsByTypeQueryHandler : IQueryHandler<GetAppointmentsByTypeQuery, PagedResult<AppointmentResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISlugGenerator _slugGenerator;
    private readonly ILogger<GetAppointmentsByTypeQueryHandler> _logger;

    public GetAppointmentsByTypeQueryHandler(
        IUnitOfWork unitOfWork,
        ISlugGenerator slugGenerator,
        ILogger<GetAppointmentsByTypeQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _slugGenerator = slugGenerator;
        _logger = logger;
    }

    public async Task<PagedResult<AppointmentResponseDto>> Handle(
        GetAppointmentsByTypeQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching appointments by type {AppointmentType}", request.AppointmentType);

        var repo = _unitOfWork.Repository<ApptEntity>();
        var skip = (request.PageNumber - 1) * request.PageSize;

        // Build filter predicate
        Func<IQueryable<ApptEntity>, IQueryable<ApptEntity>> filter = query =>
        {
            query = query.Where(a => a.AppointmentType == request.AppointmentType);

            if (request.PatientId.HasValue)
                query = query.Where(a => a.PatientId == request.PatientId);

            if (request.ProviderId.HasValue)
                query = query.Where(a => a.ProviderId == request.ProviderId);

            if (request.FromDate.HasValue)
                query = query.Where(a => a.ScheduledStart >= request.FromDate);

            if (request.ToDate.HasValue)
                query = query.Where(a => a.ScheduledStart <= request.ToDate);

            return query;
        };

        // Get total count
        var total = await repo.CountAsync(filter, cancellationToken: cancellationToken);

        // Get paged results
        var appointments = await repo.ToListAsync(
            q => filter(q).OrderByDescending(a => a.CreatedAt).Skip(skip).Take(request.PageSize),
            cancellationToken);

        // Map to DTOs and add slug
        var dtos = appointments.Adapt<List<AppointmentResponseDto>>();
        foreach (var dto in dtos)
        {
            dto.AppointmentTypeSlug = _slugGenerator.Generate(dto.AppointmentType);
            dto.Slug = dto.AppointmentTypeSlug;
            dto.SlugDisplayName = dto.AppointmentType;
        }

        return PagedResult<AppointmentResponseDto>.Create(dtos, total, request.PageNumber, request.PageSize);
    }
}

/// <summary>
/// Get appointment details by AppointmentType query handler.
/// Returns enriched appointment data including reminders and computed fields.
/// Automatically cached.
/// </summary>
public class GetAppointmentDetailsByTypeQueryHandler : IQueryHandler<GetAppointmentDetailsByTypeQuery, PagedResult<AppointmentDetailedResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISlugGenerator _slugGenerator;
    private readonly ILogger<GetAppointmentDetailsByTypeQueryHandler> _logger;

    public GetAppointmentDetailsByTypeQueryHandler(
        IUnitOfWork unitOfWork,
        ISlugGenerator slugGenerator,
        ILogger<GetAppointmentDetailsByTypeQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _slugGenerator = slugGenerator;
        _logger = logger;
    }

    public async Task<PagedResult<AppointmentDetailedResponseDto>> Handle(
        GetAppointmentDetailsByTypeQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching appointment details by type {AppointmentType}", request.AppointmentType);

        var repo = _unitOfWork.Repository<ApptEntity>();
        var skip = (request.PageNumber - 1) * request.PageSize;

        // Build filter predicate
        Func<IQueryable<ApptEntity>, IQueryable<ApptEntity>> filter = query =>
        {
            query = query.Where(a => a.AppointmentType == request.AppointmentType);

            if (request.PatientId.HasValue)
                query = query.Where(a => a.PatientId == request.PatientId);

            if (request.ProviderId.HasValue)
                query = query.Where(a => a.ProviderId == request.ProviderId);

            return query;
        };

        // Get total count
        var total = await repo.CountAsync(filter, cancellationToken: cancellationToken);

        // Get paged results
        var appointments = await repo.ToListAsync(
            q => filter(q).OrderByDescending(a => a.CreatedAt).Skip(skip).Take(request.PageSize),
            cancellationToken);

        // Map to detailed DTOs and add slug
        var dtos = appointments.Select(a => MapToDetailedDto(a)).ToList();

        return PagedResult<AppointmentDetailedResponseDto>.Create(dtos, total, request.PageNumber, request.PageSize);
    }

    private AppointmentDetailedResponseDto MapToDetailedDto(ApptEntity appointment)
    {
        var dto = new AppointmentDetailedResponseDto
        {
            Id = appointment.Id,
            PatientId = appointment.PatientId,
            ProviderId = appointment.ProviderId,
            ScheduledStart = appointment.ScheduledStart,
            ScheduledEnd = appointment.ScheduledEnd,
            AppointmentType = appointment.AppointmentType,
            Status = appointment.Status,
            ReasonForVisit = appointment.ReasonForVisit,
            Notes = appointment.Notes,
            DurationMinutes = (int)(appointment.ScheduledEnd - appointment.ScheduledStart).TotalMinutes,
            ReminderSent = appointment.ReminderSent,
            ConfirmedAt = appointment.ConfirmedAt,
            CancelledAt = appointment.CancelledAt,
            CancellationReason = appointment.CancellationReason,
            IsAvailable = appointment.Status == "Scheduled",
            TimeUntilAppointment = (appointment.ScheduledStart - DateTime.UtcNow).TotalMinutes
        };

        // Add slug
        dto.AppointmentTypeSlug = _slugGenerator.Generate(appointment.AppointmentType);
        dto.Slug = dto.AppointmentTypeSlug;
        dto.SlugDisplayName = appointment.AppointmentType;

        return dto;
    }
}
