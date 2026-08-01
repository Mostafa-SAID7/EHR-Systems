using EHRPlatform.BuildingBlocks.SharedKernel.Entities;

namespace EHRPlatform.Services.Identity.Domain.DomainEvents;

/// <summary>
/// In-process domain event raised when a user successfully enables MFA.
/// Consumed by pipeline behaviors within the Identity service.
/// For cross-service messaging, use <see cref="MfaEnabledEvent"/> (IntegrationEvent).
/// </summary>
public class MfaEnabledDomainEvent : DomainEvent
{
    public Guid UserId { get; set; }
}



