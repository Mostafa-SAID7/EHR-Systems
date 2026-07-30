using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Appointment.Application.Appointments.Mappers;
using EHRPlatform.Services.Appointment.Application.Appointments.Responses;
using EHRPlatform.Services.Appointment.Features.Appointments.Queries;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Handlers;

/// <summary>
/// Get appointment by ID handler.
/// Single Responsibility: Fetch a single appointment and project to response DTO via AppointmentMapper.
/// </summary>
public class GetAppointmentQueryHandler : IQueryHandler<GetAppointmentQuery, AppointmentResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppointmentMapper _mapper;
    private readonly ILogger<GetAppointmentQueryHandler> _logger;

    public GetAppointmentQueryHandler(
        IUnitOfWork unitOfWork,
        AppointmentMapper mapper,
        ILogger<GetAppointmentQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AppointmentResponseDto> Handle(
        GetAppointmentQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching appointment {AppointmentId}", request.AppointmentId);

        var repo = _unitOfWork.Repository<Domain.Appointment>();
        var appointment = await repo.FirstOrDefaultAsync(
            q => q.Where(a => a.Id == request.AppointmentId),
            cancellationToken);

        if (appointment == null)
            throw new InvalidOperationException($"Appointment {request.AppointmentId} not found");

        return _mapper.MapToResponseDto(appointment);
    }
}

