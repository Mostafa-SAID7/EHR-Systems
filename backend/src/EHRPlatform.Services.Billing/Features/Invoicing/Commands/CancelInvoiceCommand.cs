using EHRPlatform.BuildingBlocks.Common.Application.CQRS;

namespace EHRPlatform.Services.Billing.Features.Invoicing.Commands;

/// <summary>
/// Cancel invoice command.
/// Cancels an existing invoice, marking it as cancelled.
/// </summary>
public record CancelInvoiceCommand : ICommand
{
    public Guid InvoiceId { get; init; }
    public string Reason { get; init; } = string.Empty;
}


