namespace EHRPlatform.Services.Appointment.Application.ProviderAvailability.Responses;

/// <summary>
/// Provider availability list DTO.
/// Contains a list of provider availability slots.
/// </summary>
public class ProviderAvailabilityListDto
{
    /// <summary>Gets or sets the provider identifier.</summary>
    public Guid ProviderId { get; set; }

    /// <summary>Gets or sets the list of availability slots.</summary>
    public List<ProviderAvailabilitySlotDto> Slots { get; set; } = new();
}
