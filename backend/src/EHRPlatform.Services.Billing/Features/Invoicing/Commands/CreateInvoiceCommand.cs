using EHRPlatform.Common.Application.CQRS;
using EHRPlatform.Services.Billing.Application.Invoicing.Requests;
using EHRPlatform.Services.Billing.Application.Invoicing.Responses;

namespace EHRPlatform.Services.Billing.Features.Invoicing.Commands;

/// <summary>
/// Create invoice command.
/// Initiates creation of a new invoice with line items.
/// </summary>
public record CreateInvoiceCommand : ICommand<InvoiceResponseDto>
{
    public Guid PatientId { get; init; }
    public Guid? AppointmentId { get; init; }
    public DateTime ServiceDate { get; init; }
    public List<LineItemRequestDto> LineItems { get; init; } = new();
    public string? InsuranceProvider { get; init; }
    public string? InsurancePolicyNumber { get; init; }
    public string? Notes { get; init; }
}


