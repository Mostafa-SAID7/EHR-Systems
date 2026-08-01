using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.BuildingBlocks.Common.Messaging;
using EHRPlatform.Services.Billing.Features.Invoicing.Commands;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Billing.Features.Invoicing.Handlers;

/// <summary>
/// Cancel invoice handler.
/// Pure business logic - cancels existing invoice and publishes event.
/// </summary>
public class CancelInvoiceCommandHandler : ICommandHandler<CancelInvoiceCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ILogger<CancelInvoiceCommandHandler> _logger;

    public CancelInvoiceCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ILogger<CancelInvoiceCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task Handle(CancelInvoiceCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling invoice {InvoiceId} - Reason: {Reason}",
            command.InvoiceId, command.Reason);

        var repo = _unitOfWork.Repository<Invoice>();
        var invoice = await repo.FirstOrDefaultAsync(
            q => q.Where(i => i.Id == command.InvoiceId),
            cancellationToken);

        if (invoice == null)
            throw new InvalidOperationException($"Invoice {command.InvoiceId} not found");

        invoice.Cancel(command.Reason);
        await repo.UpdateAsync(invoice, cancellationToken);

        // Publish event
        var cancelEvent = invoice.GetDomainEvents().Last();
        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = invoice.Id,
            EventType = nameof(InvoiceCancelledEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(cancelEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Invoice {InvoiceId} cancelled successfully", command.InvoiceId);
    }
}


