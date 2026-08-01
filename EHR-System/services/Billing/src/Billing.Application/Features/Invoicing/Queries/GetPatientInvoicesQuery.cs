namespace EHRPlatform.Services.Billing.Application.Features.Invoicing.Queries;

using MediatR;

/// <summary>
/// Get all invoices for a patient with optional filtering
/// </summary>
public record GetPatientInvoicesQuery(
    Guid PatientId,
    string? Status = null,
    DateTime? FromDate = null,
    DateTime? ToDate = null,
    int PageNumber = 1,
    int PageSize = 20) : IRequest<GetPatientInvoicesResponse>;

/// <summary>
/// Response with patient invoices
/// </summary>
public record GetPatientInvoicesResponse(
    bool Success,
    string Message,
    IEnumerable<InvoiceSummaryDto> Invoices,
    int TotalCount,
    int PageNumber,
    int PageSize);

/// <summary>
/// Invoice summary for list view
/// </summary>
public record InvoiceSummaryDto(
    string InvoiceNumber,
    DateTime InvoiceDate,
    decimal TotalAmount,
    decimal PaidAmount,
    string Status);
