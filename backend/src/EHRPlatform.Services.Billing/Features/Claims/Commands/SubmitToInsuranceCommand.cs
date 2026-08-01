using EHRPlatform.BuildingBlocks.Common.Application.CQRS;

namespace EHRPlatform.Services.Billing.Features.Claims.Commands;

/// <summary>
/// Submit to insurance command.
/// Submits an invoice claim to insurance provider.
/// </summary>
public record SubmitToInsuranceCommand : ICommand
{
    public Guid InvoiceId { get; init; }
    public string InsuranceProvider { get; init; } = string.Empty;
    public string PolicyNumber { get; init; } = string.Empty;
}



