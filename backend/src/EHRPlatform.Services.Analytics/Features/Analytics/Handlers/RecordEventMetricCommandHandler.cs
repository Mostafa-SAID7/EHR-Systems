using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Services.Analytics.Domain.Entities;
using EHRPlatform.Services.Analytics.Features.Analytics.Commands;

namespace EHRPlatform.Services.Analytics.Features.Analytics.Handlers;

public class RecordEventMetricCommandHandler : ICommandHandler<RecordEventMetricCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<RecordEventMetricCommandHandler> _logger;

    public RecordEventMetricCommandHandler(IUnitOfWork unitOfWork, ILogger<RecordEventMetricCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(RecordEventMetricCommand command, CancellationToken ct)
    {
        var metric = new EventMetric
        {
            Id = Guid.NewGuid(), 
            EventType = command.EventType,
            AggregateId = command.AggregateId, 
            OccurredAt = DateTime.UtcNow,
            Properties = command.Properties
        };
        var repo = _unitOfWork.Repository<EventMetric>();
        await repo.AddAsync(metric, ct);
        await _unitOfWork.SaveChangesAsync(ct);
        _logger.LogDebug("Event metric recorded: {EventType}", command.EventType);
    }
}
