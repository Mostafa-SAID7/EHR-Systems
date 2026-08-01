using EHRPlatform.BuildingBlocks.Common.Application.CQRS;

namespace EHRPlatform.Services.Billing.Features.Claims.Commands;

/// <summary>
/// Cancel invoice command (e.g., duplicate billing).
/// </summary>
public record CancelInvoiceCommand : ICommand
{
    public Guid InvoiceId { get; init; }
    public string Reason { get; init; } = string.Empty;
}

/// <summary>
/// Handler for CancelInvoiceCommand.
/// </summary>
public class CancelInvoiceCommandHandler : ICommandHandler<CancelInvoiceCommand>
{
    private readonly EHRPlatform.Common.Data.IUnitOfWork _unitOfWork;
    private readonly ILogger<CancelInvoiceCommandHandler> _logger;

    public CancelInvoiceCommandHandler(
        EHRPlatform.Common.Data.IUnitOfWork unitOfWork,
        ILogger<CancelInvoiceCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task Handle(CancelInvoiceCommand command, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Cancelling invoice {InvoiceId}: {Reason}", command.InvoiceId, command.Reason);

        var repo = _unitOfWork.Repository<Invoice>();
        var invoice = await repo.FirstOrDefaultAsync(
            q => q.Where(i => i.Id == command.InvoiceId),
            cancellationToken);

        if (invoice == null)
            throw new KeyNotFoundException($"Invoice {command.InvoiceId} not found");

        invoice.Status = "Cancelled";
        invoice.Notes = string.IsNullOrEmpty(command.Reason)
            ? invoice.Notes
            : $"Cancelled: {command.Reason}";

        await repo.UpdateAsync(invoice, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Invoice {InvoiceId} cancelled", command.InvoiceId);
    }
}


