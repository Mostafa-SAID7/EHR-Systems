using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.BuildingBlocks.Common.Messaging;
using EHRPlatform.Services.Billing.Features.Invoicing.Commands;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Billing.Features.Invoicing.Handlers;

/// <summary>
/// Create invoice handler.
/// Pure business logic - no mapping responsibility.
/// </summary>
public class CreateInvoiceCommandHandler : ICommandHandler<CreateInvoiceCommand, InvoiceResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IOutboxRepository _outbox;
    private readonly InvoiceMapper _mapper;
    private readonly ILogger<CreateInvoiceCommandHandler> _logger;

    public CreateInvoiceCommandHandler(
        IUnitOfWork unitOfWork,
        IOutboxRepository outbox,
        InvoiceMapper mapper,
        ILogger<CreateInvoiceCommandHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _outbox = outbox;
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
            DueDate = command.ServiceDate.AddDays(30), // 30-day payment terms
            Status = "Draft",
            InsuranceProvider = command.InsuranceProvider,
            InsurancePolicyNumber = command.InsurancePolicyNumber,
            Notes = command.Notes
        };

        // Add line items
        foreach (var item in command.LineItems)
        {
            invoice.AddLineItem(item.Description, item.CPTCode, item.Quantity, item.UnitPrice);
        }

        // Calculate totals
        invoice.CalculateTotals();

        var repo = _unitOfWork.Repository<Invoice>();
        await repo.AddAsync(invoice, cancellationToken);

        // Publish event
        var createdEvent = new InvoiceCreatedEvent(
            invoice.Id, invoice.PatientId, invoice.TotalAmount, invoiceNumber);

        await _outbox.AddAsync(new OutboxEvent
        {
            Id = Guid.NewGuid(),
            AggregateId = invoice.Id,
            EventType = nameof(InvoiceCreatedEvent),
            EventData = System.Text.Json.JsonSerializer.Serialize(createdEvent),
            CreatedAt = DateTime.UtcNow
        }, cancellationToken);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Invoice created {InvoiceId} (#{Number})", invoice.Id, invoiceNumber);

        // Delegate mapping to mapper
        return _mapper.MapToResponseDto(invoice);
    }

    private static string GenerateInvoiceNumber()
    {
        // Format: INV-YYYYMMDD-XXXXXX
        var timestamp = DateTime.UtcNow.ToString("yyyyMMdd");
        var random = new Random().Next(100000, 999999);
        return $"INV-{timestamp}-{random}";
    }
}


