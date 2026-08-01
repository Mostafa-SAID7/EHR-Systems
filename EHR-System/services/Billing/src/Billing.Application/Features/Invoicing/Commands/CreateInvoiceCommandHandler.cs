using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Billing.Application.Features.Invoicing.Commands;
using EHRPlatform.Services.Billing.Application.Features.Invoicing.Mappers;
using EHRPlatform.Services.Billing.Contracts.Responses;
using EHRPlatform.Services.Billing.Domain.Entities;
using EHRPlatform.Services.Billing.Domain.Events;
using EHRPlatform.Services.Billing.Persistence;
using EHRPlatform.Services.Billing.Persistence.Repositories;

namespace EHRPlatform.Services.Billing.Application.Features.Invoicing.Handlers;

/// <summary>
/// Handler for creating a new invoice.
/// Applies business rules, calculates totals, and publishes domain events.
/// HIPAA Compliant: All invoice operations are logged for audit trails.
/// </summary>
public class CreateInvoiceCommandHandler : IRequestHandler<CreateInvoiceCommand, InvoiceResponseDto>
{
    private readonly BillingContext _context;
    private readonly InvoiceMapper _mapper;
    private readonly ILogger<CreateInvoiceCommandHandler> _logger;

    public CreateInvoiceCommandHandler(
        BillingContext context,
        InvoiceMapper mapper,
        ILogger<CreateInvoiceCommandHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger;
    }

    public async Task<InvoiceResponseDto> Handle(
        CreateInvoiceCommand command,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating invoice for patient {PatientId}", command.PatientId);

        var invoiceNumber = GenerateInvoiceNumber();
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            PatientId = command.PatientId,
            AppointmentId = command.AppointmentId,
            InvoiceNumber = invoiceNumber,
            ServiceDate = command.ServiceDate,
            DueDate = command.ServiceDate.AddDays(30),
            Status = "Draft",
            InsuranceProvider = command.InsuranceProvider,
            InsurancePolicyNumber = command.InsurancePolicyNumber,
            Notes = command.Notes,
            CreatedAt = DateTime.UtcNow
        };

        // Add line items
        foreach (var item in command.LineItems)
        {
            invoice.AddLineItem(item.Description, item.CPTCode, item.Quantity, item.UnitPrice);
        }

        // Calculate totals
        invoice.CalculateTotals();

        // Publish event
        invoice.RaiseEvent(new InvoiceCreatedEvent(
            invoice.Id, invoice.PatientId, invoice.TotalAmount, invoiceNumber));

        await _context.Invoices.AddAsync(invoice, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Invoice created {InvoiceId} (#{InvoiceNumber})", invoice.Id, invoiceNumber);

        return _mapper.MapToResponseDto(invoice);
    }

    private static string GenerateInvoiceNumber()
    {
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(100000, 999999);
        return $"INV-{timestamp}-{random}";
    }
}
