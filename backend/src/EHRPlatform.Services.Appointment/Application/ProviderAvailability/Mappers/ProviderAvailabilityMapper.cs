using Mapster;
using EHRPlatform.Common.Mapping;
using EHRPlatform.Services.Appointment.Application.ProviderAvailability.Responses;
using Microsoft.Extensions.Logging;
using ProvAvailEntity = EHRPlatform.Services.Appointment.Features.Appointments.Domain.ProviderAvailability;

namespace EHRPlatform.Services.Appointment.Application.ProviderAvailability.Mappers;

/// <summary>
/// Provider availability mapper.
/// Converts ProviderAvailability domain entities to DTOs.
/// Single Responsibility: Mapping ProviderAvailability aggregate to application layer DTOs.
/// </summary>
public class ProviderAvailabilityMapper : MappingServiceBase<ProvAvailEntity, ProviderAvailabilityDto>
{
    public ProviderAvailabilityMapper(ILogger<ProviderAvailabilityMapper> logger) : base(logger) { }

    /// <summary>
    /// Maps provider availability entity to response DTO.
    /// </summary>
    public ProviderAvailabilityDto MapToResponseDto(ProvAvailEntity availability) => MapSingleToDto(availability);

    /// <summary>
    /// Maps collection of availability slots to response DTOs.
    /// </summary>
    public List<ProviderAvailabilityDto> MapToResponseDtoList(ICollection<ProvAvailEntity> slots)
    {
        Logger.LogDebug("Mapping {Count} availability slots to response DTO list", slots.Count);
        return slots.Adapt<List<ProviderAvailabilityDto>>();
    }

    /// <summary>
    /// Maps provider availability slots to list DTO with slot details.
    /// </summary>
    public ProviderAvailabilityListDto MapToAvailabilityListDto(Guid providerId, IList<ProvAvailEntity> slots)
    {
        Logger.LogDebug("Mapping {Count} availability slots for provider {ProviderId} to list DTO", 
            slots.Count, providerId);
        
        return new()
        {
            ProviderId = providerId,
            Slots = slots.Select(s => new ProviderAvailabilitySlotDto
            {
                Id = s.Id,
                SlotStart = s.SlotStart,
                SlotEnd = s.SlotEnd,
                IsRecurring = s.IsRecurring,
                RecurrencePattern = s.RecurrencePattern,
                MaxAppointmentsPerSlot = s.MaxAppointmentsPerSlot,
                CurrentBookings = s.CurrentBookings,
                HasAvailability = s.HasAvailability()
            }).ToList()
        };
    }
}
