using EHRPlatform.BuildingBlocks.SharedKernel.Entities;

namespace EHRPlatform.Services.Identity.Domain.DomainEvents;

/// <summary>
/// In-process domain event raised when an admin creates a new user.
/// Consumed by pipeline behaviors within the Identity service.
/// For cross-service messaging, use <see cref="UserCreatedEvent"/> (IntegrationEvent).
/// </summary>
public class UserCreatedDomainEvent : DomainEvent
{
    public Guid   UserId { get; set; }
    public string Email  { get; set; } = string.Empty;
    public string Role   { get; set; } = string.Empty;
}



