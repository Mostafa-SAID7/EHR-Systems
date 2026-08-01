namespace EHRPlatform.Services.Billing.Application.Features.Invoicing.Commands;

using MediatR;

/// <summary>
/// Update invoice details (status, notes, payment info)
/// </summary>
public record UpdateInvoiceCommand(
    string InvoiceNumber,
    string? Notes = null,
    string? Status = null,
    decimal? PaidAmount = null) : IRequest<UpdateInvoiceResponse>;

/// <summary>
/// Response from updating invoice
/// </summary>
public record UpdateInvoiceResponse(
    bool Success,
    string Message,
    string? InvoiceNumber = null);
