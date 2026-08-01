namespace EHRPlatform.Services.Billing.Application.Features.Invoicing.Commands;

using MediatR;

/// <summary>
/// Cancel an invoice with reason
/// </summary>
public record CancelInvoiceCommand(
    string InvoiceNumber,
    string Reason) : IRequest<CancelInvoiceResponse>;

/// <summary>
/// Response from cancelling invoice
/// </summary>
public record CancelInvoiceResponse(
    bool Success,
    string Message,
    string? InvoiceNumber = null);
