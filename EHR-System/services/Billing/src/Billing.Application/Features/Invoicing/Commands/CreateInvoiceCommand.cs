using MediatR;
using EHRPlatform.Services.Billing.Contracts.Responses;
using EHRPlatform.Services.Billing.Contracts.Requests;

namespace EHRPlatform.Services.Billing.Application.Features.Invoicing.Commands;

/// <summary>
/// Create a new invoice command.
/// </summary>
public record CreateInvoiceCommand(
    Guid PatientId,
    Guid? AppointmentId,
    DateTime ServiceDate,
    string? InsuranceProvider,
    string? InsurancePolicyNumber,
    string? Notes,
    List<LineItemRequestDto> LineItems) : IRequest<InvoiceResponseDto>
{
}
