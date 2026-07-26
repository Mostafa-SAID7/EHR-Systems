using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Appointment.Application.AppointmentManagement.Mappers;
using EHRPlatform.Services.Appointment.Application.AppointmentManagement.Responses;
using EHRPlatform.Services.Appointment.Features.Appointments.Domain;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Appointment.Features.Appointments.Queries;

/// <summary>
/// Get provider availability slots handler.
/// Single Responsibility: Retrieve active availability slots for a provider within a date range.
/// </summary>
public class GetProviderAvailabilityQueryHandler : IQueryHandler<GetProviderAvailabilityQuery, ProviderAvailabilityListDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly AppointmentMapper _mapper;
    private readonly ILogger<GetProviderAvailabilityQueryHandler> _logger;

    public GetProviderAvailabilityQueryHandler(
        IUnitOfWork unitOfWork,
        AppointmentMapper mapper,
        ILogger<GetProviderAvailabilityQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProviderAvailabilityListDto> Handle(
        GetProviderAvailabilityQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Fetching availability for provider {ProviderId} from {From} to {To}",
            request.ProviderId, request.FromDate, request.ToDate);

        var repo = _unitOfWork.Repository<ProviderAvailability>();
        var slots = await repo.ToListAsync(
            q => q.Where(a =>
                a.ProviderId == request.ProviderId &&
                a.IsActive &&
                a.SlotStart >= request.FromDate &&
                a.SlotEnd <= request.ToDate)
                .OrderBy(a => a.SlotStart),
            cancellationToken);

        return _mapper.MapToAvailabilityListDto(request.ProviderId, slots);
    }
}
