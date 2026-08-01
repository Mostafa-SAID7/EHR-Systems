using EHRPlatform.BuildingBlocks.Common.Application.CQRS;

namespace EHRPlatform.Services.Billing.Features.Payments.Commands;

/// <summary>
/// Record payment command.
/// </summary>
public record RecordPaymentCommand : ICommand
{
    public Guid InvoiceId { get; init; }
    public decimal Amount { get; init; }
    public string Method { get; init; } = string.Empty; // Credit Card, Check, ACH, Insurance
    public string? Reference { get; init; }
}


