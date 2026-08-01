using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Identity.Domain.Events;

public class PasswordChangedEvent : IntegrationEvent
{
    public new Guid UserId { get; set; }
    public string Email { get; set; } = string.Empty;

    public PasswordChangedEvent() { }

    public PasswordChangedEvent(Guid id, string email)
    {
        UserId = id;
        Email  = email;
    }
}

