using MediatR;
using EHRPlatform.Services.Billing.Contracts.Responses;

namespace EHRPlatform.Services.Billing.Application.Features.Invoicing.Queries;

/// <summary>
/// Get invoice by invoice number query.
/// </summary>
public record GetInvoiceByNumberQuery(string InvoiceNumber) : IRequest<InvoiceResponseDto?>
{
}
