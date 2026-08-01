using Mapster;
using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.Services.Appointment.Application.ProviderAvailability.Mappers;
using EHRPlatform.Services.Appointment.Application.ProviderAvailability.Responses;
using EHRPlatform.Services.Appointment.Features.ProviderAvailability.Commands;
using ProviderAvailEntity = EHRPlatform.Services.Appointment.Features.Appointments.Domain.ProviderAvailability;

namespace EHRPlatform.Services.Appointment.Features.ProviderAvailability.Handlers;

/// <summary>
/// Set provider availability command handler.
/// Creates or updates availability slots for healthcare providers.
/// Single Responsibility: Persist availability slots.
/// </summary>
public class SetProviderAvailabilityCommandHandler : ICommandHandler<SetProviderAvailabilityCommand, ProviderAvailabilityDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ProviderAvailabilityMapper _mapper;
    private readonly ILogger<SetProviderAvailabilityCommandHandler> _logger;

    public SetProviderAvailabilityCommandHandler(
        IUnitOfWork unitOfWork,
        ProviderAvailabilityMapper mapper,
        ILogger<SetProviderAvailabilityCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<ProviderAvailabilityDto> Handle(
        SetProviderAvailabilityCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation(
            "Setting availability for provider {ProviderId}: {Start} - {End}",
            command.ProviderId, command.SlotStart, command.SlotEnd);

        var availSlot = new ProviderAvailEntity
        {
            Id = Guid.NewGuid(),
            ProviderId = command.ProviderId,
            SlotStart = command.SlotStart,
            SlotEnd = command.SlotEnd,
            IsRecurring = command.IsRecurring,
            RecurrencePattern = command.RecurrencePattern,
            MaxAppointmentsPerSlot = command.MaxAppointmentsPerSlot,
            CurrentBookings = 0,
            IsActive = true
        };

        var repo = _unitOfWork.Repository<ProviderAvailEntity>();
        await repo.AddAsync(availSlot, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return availSlot.Adapt<ProviderAvailabilityDto>();
    }
}



