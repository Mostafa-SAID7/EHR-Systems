using EHRPlatform.BuildingBlocks.Common.Events;

namespace EHRPlatform.Services.Analytics.Domain.Events;

public class ReportExecutedEvent : IntegrationEvent
{
    public Guid ReportId { get; set; }
    public Guid ExecutionId { get; set; }
    public string Status { get; set; }
    public int RecordCount { get; set; }

    public ReportExecutedEvent(Guid reportId, Guid executionId, string status, int count)
    {
        ReportId = reportId;
        ExecutionId = executionId;
        Status = status;
        RecordCount = count;
    }
}

