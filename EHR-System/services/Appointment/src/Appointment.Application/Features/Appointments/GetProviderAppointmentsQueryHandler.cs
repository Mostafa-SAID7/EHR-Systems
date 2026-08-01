using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.Services.Appointment.Application.Appointments.Mappers;
using EHRPlatform.Services.Appointment.Application.Appointments.Responses;
using EHRPlatform.Services.Appointment.Features.Appointments.Domain;
using EHRPlatform.Services.Appointment.Features.Appointments.Queries;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Handlers;

/// <summary>
/// Get provider appointment calendar handler.
/// Single Responsibility: Retrieve all appointments for a provider on a specific calendar date.
/// </summary>
public class GetProviderAppointmentsQueryHandler : IQueryHandler<GetProviderAppointmentsQuery, ProviderAppointmentCalendarDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppointmentMapper _mapper;
    private readonly ILogger<GetProviderAppointmentsQueryHandler> _logger;

    public GetProviderAppointmentsQueryHandler(
        IUnitOfWork unitOfWork,
        AppointmentMapper mapper,
        ILogger<GetProviderAppointmentsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProviderAppointmentCalendarDto> Handle(
        GetProviderAppointmentsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Fetching calendar for provider {ProviderId} on {Date:yyyy-MM-dd}",
            request.ProviderId, request.CalendarDate);

        var dayStart = request.CalendarDate.Date;
        var dayEnd = dayStart.AddDays(1);

        var appointmentRepo = _unitOfWork.Repository<Domain.Appointment>();
        var query = appointmentRepo.Query()
            .Where(a => a.ProviderId == request.ProviderId &&
                       a.ScheduledStart >= dayStart &&
                       a.ScheduledStart < dayEnd);

        if (!string.IsNullOrEmpty(request.StatusFilter))
            query = query.Where(a => a.Status == request.StatusFilter);

        var appointments = await query
            .OrderBy(a => a.ScheduledStart)
            .ToListAsync(cancellationToken);

        return _mapper.MapToProviderCalendarDto(request.ProviderId, request.CalendarDate, appointments);
    }
}



