using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Appointment.Application.AppointmentManagement.Mappers;
using EHRPlatform.Services.Appointment.Application.AppointmentManagement.Responses;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Queries;

/// <summary>
/// Get patient appointments handler.
/// Single Responsibility: Retrieve paginated appointments for a patient within a date range.
/// </summary>
public class GetPatientAppointmentsQueryHandler : IQueryHandler<GetPatientAppointmentsQuery, AppointmentListDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppointmentMapper _mapper;
    private readonly ILogger<GetPatientAppointmentsQueryHandler> _logger;

    public GetPatientAppointmentsQueryHandler(
        IUnitOfWork unitOfWork,
        AppointmentMapper mapper,
        ILogger<GetPatientAppointmentsQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<AppointmentListDto> Handle(
        GetPatientAppointmentsQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching appointments for patient {PatientId}", request.PatientId);

        var repo = _unitOfWork.Repository<Domain.Appointment>();
        var skip = (request.PageNumber - 1) * request.PageSize;

        var total = await repo.CountAsync(
            q => q.Where(a => a.PatientId == request.PatientId),
            cancellationToken);

        var appointments = await repo.ToListAsync(
            q => q.Where(a => a.PatientId == request.PatientId)
                .Where(a => a.ScheduledStart >= (request.FromDate ?? DateTime.MinValue))
                .Where(a => a.ScheduledStart <= (request.ToDate ?? DateTime.MaxValue))
                .OrderByDescending(a => a.ScheduledStart)
                .Skip(skip)
                .Take(request.PageSize),
            cancellationToken);

        return _mapper.MapToListDto(appointments, total, request.PageNumber, request.PageSize);
    }
}
