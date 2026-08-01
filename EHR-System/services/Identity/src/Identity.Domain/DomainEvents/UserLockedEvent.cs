using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Identity.Domain.DomainEvents;

public class UserLockedEvent : IntegrationEvent
{
    public new Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;

    public UserLockedEvent() { }

    public UserLockedEvent(Guid id, string email, string reason = "Failed login attempts")
    {
        UserId = id;
        Email  = email;
        Reason = reason;
    }
}


