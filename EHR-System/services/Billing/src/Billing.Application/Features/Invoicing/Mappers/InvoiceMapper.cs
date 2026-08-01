using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Billing.Contracts.Responses;
using EHRPlatform.Services.Billing.Domain.Entities;

namespace EHRPlatform.Services.Billing.Application.Features.Invoicing.Mappers;

/// <summary>
/// Maps Invoice domain model to DTOs for API responses.
/// </summary>
public class InvoiceMapper
{
    private readonly ILogger<InvoiceMapper> _logger;

    public InvoiceMapper(ILogger<InvoiceMapper> logger)
    {
        _logger = logger;
    }

    public InvoiceResponseDto MapToResponseDto(Invoice invoice)
    {
        return new InvoiceResponseDto
        {
            Id = invoice.Id,
            PatientId = invoice.PatientId,
            AppointmentId = invoice.AppointmentId,
            InvoiceNumber = invoice.InvoiceNumber,
            ServiceDate = invoice.ServiceDate,
            DueDate = invoice.DueDate,
            Status = invoice.Status,
            SubTotal = invoice.SubTotal,
            TaxAmount = invoice.TaxAmount,
            InsuranceResponsibility = invoice.InsuranceResponsibility,
            PatientResponsibility = invoice.PatientResponsibility,
            TotalAmount = invoice.TotalAmount,
            AmountPaid = invoice.AmountPaid,
            BalanceDue = invoice.BalanceDue,
            InsuranceProvider = invoice.InsuranceProvider,
            InsurancePolicyNumber = invoice.InsurancePolicyNumber,
            Notes = invoice.Notes,
            LineItems = invoice.LineItems.Select(MapLineItem).ToList(),
            Payments = invoice.Payments.Select(MapPayment).ToList(),
            CreatedAt = invoice.CreatedAt,
            UpdatedAt = invoice.UpdatedAt
        };
    }

    private static LineItemDto MapLineItem(LineItem item)
    {
        return new LineItemDto
        {
            Id = item.Id,
            Description = item.Description,
            CPTCode = item.CPTCode,
            Quantity = item.Quantity,
            UnitPrice = item.UnitPrice,
            Amount = item.Amount
        };
    }

    private static PaymentDto MapPayment(Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,
            Amount = payment.Amount,
            Method = payment.Method,
            Reference = payment.Reference,
            ReceivedAt = payment.ReceivedAt
        };
    }

    public List<InvoiceResponseDto> MapToResponseDtoList(IEnumerable<Invoice> invoices)
    {
        return invoices.Select(MapToResponseDto).ToList();
    }
}
