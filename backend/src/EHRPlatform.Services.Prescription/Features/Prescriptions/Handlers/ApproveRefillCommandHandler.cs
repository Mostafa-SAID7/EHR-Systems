using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Messaging;
using EHRPlatform.Services.Prescription.Features.Prescriptions.Commands;
using EHRPlatform.Services.Prescription.Domain.Entities;

namespace EHRPlatform.Services.Prescription.Features.Prescriptions.Handlers;

/// <summary>
/// Approve refill handler.
/// </summary>
public class ApproveRefillCommandHandler : ICommandHandler<ApproveRefillCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ILogger<ApproveRefillCommandHandler> _logger;

    public ApproveRefillCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ILogger<ApproveRefillCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task Handle(ApproveRefillCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Approving refill {RefillId} for prescription {PrescriptionId}",
            command.RefillId, command.PrescriptionId);

        var repo = _unitOfWork.Repository<PrescriptionEntity>();
        var prescription = await repo.FirstOrDefaultAsync(
            q => q.Where(p => p.Id == command.PrescriptionId),
            cancellationToken);

        if (prescription == null)
            throw new InvalidOperationException($"Prescription {command.PrescriptionId} not found");

        prescription.ApproveRefill(command.RefillId);
        await repo.UpdateAsync(prescription, cancellationToken);

        // Publish event
        var approveEvent = prescription.GetDomainEvents().Last();
        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = prescription.Id,
            EventType = nameof(PrescriptionRefillApprovedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(approveEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

