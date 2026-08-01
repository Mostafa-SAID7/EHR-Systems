using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Analytics.Domain.Events;

public class ReportScheduledEvent : IntegrationEvent
{
    public Guid ReportId { get; set; }
    public string ReportName { get; set; }
    public string Schedule { get; set; }

    public ReportScheduledEvent(Guid id, string name, string schedule)
    {
        ReportId = id;
        ReportName = name;
        Schedule = schedule;
    }
}

