using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Common.Data;
using EHRPlatform.Common.Messaging;
using EHRPlatform.Services.Billing.Domain;
using EHRPlatform.Services.Billing.Features.Payments.Commands;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Billing.Features.Payments.Handlers;

/// <summary>
/// Record payment handler.
/// Pure business logic - no mapping responsibility.
/// </summary>
public class RecordPaymentCommandHandler : ICommandHandler<RecordPaymentCommand>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly ILogger<RecordPaymentCommandHandler> _logger;

    public RecordPaymentCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        ILogger<RecordPaymentCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
        _logger = logger;
    }

    public async Task Handle(RecordPaymentCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recording payment {Amount} for invoice {InvoiceId}",
            command.Amount, command.InvoiceId);

        var repo = _unitOfWork.Repository<Invoice>();
        var invoice = await repo.FirstOrDefaultAsync(
            q => q.Where(i => i.Id == command.InvoiceId),
            cancellationToken);

        if (invoice == null)
            throw new InvalidOperationException($"Invoice {command.InvoiceId} not found");

        invoice.RecordPayment(command.Amount, command.Method, command.Reference ?? "");

        await repo.UpdateAsync(invoice, cancellationToken);

        // Publish event
        var paymentEvent = invoice.GetDomainEvents().Last();
        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = invoice.Id,
            EventType = nameof(PaymentReceivedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(paymentEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Payment recorded for invoice {InvoiceId}", command.InvoiceId);
    }
}

