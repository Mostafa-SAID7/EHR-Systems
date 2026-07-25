using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Messaging;
using EHRPlatform.Services.Prescription.Features.Prescriptions.Commands;
using EHRPlatform.Services.Prescription.Domain.Entities;

namespace EHRPlatform.Services.Prescription.Features.Prescriptions.Handlers;

/// <summary>
/// Request refill handler.
/// </summary>
public class RequestRefillCommandHandler : ICommandHandler<RequestRefillCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ILogger<RequestRefillCommandHandler> _logger;

    public RequestRefillCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ILogger<RequestRefillCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task Handle(RequestRefillCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Requesting refill for prescription {PrescriptionId}", command.PrescriptionId);

        var repo = _unitOfWork.Repository<Prescription>();
        var prescription = await repo.FirstOrDefaultAsync(
            q => q.Where(p => p.Id == command.PrescriptionId),
            cancellationToken);

        if (prescription == null)
            throw new InvalidOperationException($"Prescription {command.PrescriptionId} not found");

        prescription.RequestRefill(command.PharmacyId ?? "");
        await repo.UpdateAsync(prescription, cancellationToken);

        // Publish event
        var refillEvent = prescription.GetDomainEvents().Last();
        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = prescription.Id,
            EventType = nameof(PrescriptionRefillRequestedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(refillEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}
