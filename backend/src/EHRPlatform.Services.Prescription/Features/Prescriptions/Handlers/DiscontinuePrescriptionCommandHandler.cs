using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Messaging;
using EHRPlatform.Services.Prescription.Features.Prescriptions.Commands;
using EHRPlatform.Services.Prescription.Domain.Entities;

namespace EHRPlatform.Services.Prescription.Features.Prescriptions.Handlers;

/// <summary>
/// Discontinue prescription handler.
/// </summary>
public class DiscontinuePrescriptionCommandHandler : ICommandHandler<DiscontinuePrescriptionCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ILogger<DiscontinuePrescriptionCommandHandler> _logger;

    public DiscontinuePrescriptionCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ILogger<DiscontinuePrescriptionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task Handle(DiscontinuePrescriptionCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Discontinuing prescription {PrescriptionId}", command.PrescriptionId);

        var repo = _unitOfWork.Repository<PrescriptionEntity>();
        var prescription = await repo.FirstOrDefaultAsync(
            q => q.Where(p => p.Id == command.PrescriptionId),
            cancellationToken);

        if (prescription == null)
            throw new InvalidOperationException($"Prescription {command.PrescriptionId} not found");

        prescription.Discontinue(command.Reason);
        await repo.UpdateAsync(prescription, cancellationToken);

        // Publish event
        var discontinueEvent = prescription.GetDomainEvents().Last();
        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = prescription.Id,
            EventType = nameof(PrescriptionDiscontinuedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(discontinueEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}


