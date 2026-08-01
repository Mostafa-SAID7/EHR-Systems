using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.Services.Appointment.Application.ProviderAvailability.Responses;

namespace EHRPlatform.Services.Appointment.Features.ProviderAvailability.Commands;

/// <summary>
/// Set provider availability command.
/// </summary>
public record SetProviderAvailabilityCommand : ICommand<ProviderAvailabilityDto>
{
    public Guid ProviderId { get; init; }
    public DateTime SlotStart { get; init; }
    public DateTime SlotEnd { get; init; }
    public bool IsRecurring { get; init; }
    public string? RecurrencePattern { get; init; }
    public int? MaxAppointmentsPerSlot { get; init; }
}


