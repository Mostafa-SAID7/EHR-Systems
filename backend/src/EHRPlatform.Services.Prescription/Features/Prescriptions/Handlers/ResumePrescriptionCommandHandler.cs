using EHRPlatform.Common.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Messaging;
using EHRPlatform.Services.Prescription.Features.Prescriptions.Commands;
using EHRPlatform.Services.Prescription.Domain.Entities;

namespace EHRPlatform.Services.Prescription.Features.Prescriptions.Handlers;

/// <summary>
/// Resume prescription handler.
/// </summary>
public class ResumePrescriptionCommandHandler : ICommandHandler<ResumePrescriptionCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ILogger<ResumePrescriptionCommandHandler> _logger;

    public ResumePrescriptionCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ILogger<ResumePrescriptionCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task Handle(ResumePrescriptionCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Resuming prescription {PrescriptionId}", command.PrescriptionId);

        var repo = _unitOfWork.Repository<PrescriptionEntity>();
        var prescription = await repo.FirstOrDefaultAsync(
            q => q.Where(p => p.Id == command.PrescriptionId),
            cancellationToken);

        if (prescription == null)
            throw new InvalidOperationException($"Prescription {command.PrescriptionId} not found");

        prescription.Resume();
        await repo.UpdateAsync(prescription, cancellationToken);

        // Publish event
        var resumeEvent = prescription.GetDomainEvents().Last();
        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = prescription.Id,
            EventType = nameof(PrescriptionResumedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(resumeEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

