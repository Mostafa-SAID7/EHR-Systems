using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Appointment.Application.AppointmentManagement.Mappers;
using EHRPlatform.Services.Appointment.Application.AppointmentManagement.Responses;
using EHRPlatform.Services.Appointment.Features.Appointments.Domain;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Queries;

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
            request.ProviderId, request.Date);

        var dayStart = request.Date.Date;
        var dayEnd = dayStart.AddDays(1);

        var appointmentRepo = _unitOfWork.Repository<Domain.Appointment>();
        var appointments = await appointmentRepo.ToListAsync(
            q => q.Where(a =>
                a.ProviderId == request.ProviderId &&
                a.ScheduledStart >= dayStart &&
                a.ScheduledStart < dayEnd)
                .OrderBy(a => a.ScheduledStart),
            cancellationToken);

        return _mapper.MapToProviderCalendarDto(request.ProviderId, request.Date, appointments);
    }
}
