using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Shared.DTOs;
using EHRPlatform.Services.Appointment.Application.Appointments.Mappers;
using EHRPlatform.Services.Appointment.Application.Appointments.Responses;
using EHRPlatform.Services.Appointment.Features.Appointments.Queries;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Handlers;

/// <summary>
/// Get appointments by type handler.
/// Single Responsibility: Retrieve appointments filtered by appointment type.
/// </summary>
public class GetAppointmentsByTypeQueryHandler : IQueryHandler<GetAppointmentsByTypeQuery, PagedResult<AppointmentResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppointmentMapper _mapper;
    private readonly ILogger<GetAppointmentsByTypeQueryHandler> _logger;

    public GetAppointmentsByTypeQueryHandler(
        IUnitOfWork unitOfWork,
        AppointmentMapper mapper,
        ILogger<GetAppointmentsByTypeQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<PagedResult<AppointmentResponseDto>> Handle(
        GetAppointmentsByTypeQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting appointments by type: {Type}", request.AppointmentType);

        var repo = _unitOfWork.Repository<Domain.Appointment>();
        var query = repo.Query()
            .Where(a => a.AppointmentType == request.AppointmentType);

        // Apply optional filters
        if (request.PatientId.HasValue)
            query = query.Where(a => a.PatientId == request.PatientId.Value);

        if (request.ProviderId.HasValue)
            query = query.Where(a => a.ProviderId == request.ProviderId.Value);

        var total = await query.CountAsync(cancellationToken);
        var appointments = await query
            .OrderByDescending(a => a.ScheduledStart)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return _mapper.MapToPagedResult(appointments, total, request.PageNumber, request.PageSize);
    }
}

