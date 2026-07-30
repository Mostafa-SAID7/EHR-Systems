using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Messaging;
using EHRPlatform.Services.Prescription.Features.Prescriptions.Commands;
using EHRPlatform.Services.Prescription.Domain.Entities;

namespace EHRPlatform.Services.Prescription.Features.Prescriptions.Handlers;

/// <summary>
/// Suspend prescription handler.
/// </summary>
public class SuspendPrescriptionCommandHandler : ICommandHandler<SuspendPrescriptionCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ILogger<SuspendPrescriptionCommandHandler> _logger;

    public SuspendPrescriptionCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ILogger<SuspendPrescriptionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task Handle(SuspendPrescriptionCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Suspending prescription {PrescriptionId}", command.PrescriptionId);

        var repo = _unitOfWork.Repository<PrescriptionEntity>();
        var prescription = await repo.FirstOrDefaultAsync(
            q => q.Where(p => p.Id == command.PrescriptionId),
            cancellationToken);

        if (prescription == null)
            throw new InvalidOperationException($"Prescription {command.PrescriptionId} not found");

        prescription.Suspend(command.Reason);
        await repo.UpdateAsync(prescription, cancellationToken);

        // Publish event
        var suspendEvent = prescription.GetDomainEvents().Last();
        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = prescription.Id,
            EventType = nameof(PrescriptionSuspendedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(suspendEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}


