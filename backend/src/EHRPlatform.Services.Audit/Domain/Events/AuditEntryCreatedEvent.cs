using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Audit.Domain.Events;

public class AuditEntryCreatedEvent : IntegrationEvent
{
    public Guid AuditId { get; set; }
    public Guid UserId { get; set; }
    public string Action { get; set; }
    public string ResourceType { get; set; }
    public Guid ResourceId { get; set; }

    public AuditEntryCreatedEvent(Guid id, Guid userId, string action, string resourceType, Guid resourceId)
    {
        AuditId = id;
        UserId = userId;
        Action = action;
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}

