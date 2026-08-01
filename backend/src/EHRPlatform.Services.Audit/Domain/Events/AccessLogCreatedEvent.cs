using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Audit.Domain.Events;

public class AccessLogCreatedEvent : IntegrationEvent
{
    public Guid AccessLogId { get; set; }
    public Guid UserId { get; set; }
    public string ResourceType { get; set; }
    public Guid ResourceId { get; set; }

    public AccessLogCreatedEvent(Guid id, Guid userId, string resourceType, Guid resourceId)
    {
        AccessLogId = id;
        UserId = userId;
        ResourceType = resourceType;
        ResourceId = resourceId;
    }
}

