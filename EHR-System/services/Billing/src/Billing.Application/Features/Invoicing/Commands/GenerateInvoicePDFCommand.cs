namespace EHRPlatform.Services.Billing.Application.Features.Invoicing.Commands;

using MediatR;

/// <summary>
/// Generate PDF for an invoice
/// </summary>
public record GenerateInvoicePDFCommand(
    string InvoiceNumber) : IRequest<GenerateInvoicePDFResponse>;

/// <summary>
/// Response with PDF generation result
/// </summary>
public record GenerateInvoicePDFResponse(
    bool Success,
    string Message,
    byte[]? PdfContent = null,
    string? FileName = null);
