using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Appointment.Application.ProviderAvailability.Mappers;
using EHRPlatform.Services.Appointment.Application.ProviderAvailability.Responses;
using EHRPlatform.Services.Appointment.Features.ProviderAvailability.Queries;
using ProviderAvailEntity = EHRPlatform.Services.Appointment.Features.Appointments.Domain.ProviderAvailability;

namespace EHRPlatform.Services.Appointment.Features.ProviderAvailability.Handlers;

/// <summary>
/// Get provider availability slots query handler.
/// Retrieves active availability slots for a provider within a date range.
/// Single Responsibility: Query and map availability slots.
/// </summary>
public class GetProviderAvailabilityQueryHandler : IQueryHandler<GetProviderAvailabilityQuery, ProviderAvailabilityListDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ProviderAvailabilityMapper _mapper;
    private readonly ILogger<GetProviderAvailabilityQueryHandler> _logger;

    public GetProviderAvailabilityQueryHandler(
        IUnitOfWork unitOfWork,
        ProviderAvailabilityMapper mapper,
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

        var repo = _unitOfWork.Repository<ProviderAvailEntity>();
        var query = repo.Query()
            .Where(a => a.ProviderId == request.ProviderId && a.IsActive);

        // Apply date range filter
        query = query.Where(a => a.SlotStart >= request.FromDate && a.SlotEnd <= request.ToDate);

        // Apply optional appointment type filter
        if (!string.IsNullOrEmpty(request.AppointmentType))
        {
            // Adjust based on your ProviderAvailability entity structure
        }

        var slots = await query
            .OrderBy(a => a.SlotStart)
            .ToListAsync(cancellationToken);

        return _mapper.MapToAvailabilityListDto(request.ProviderId, slots);
    }
}
