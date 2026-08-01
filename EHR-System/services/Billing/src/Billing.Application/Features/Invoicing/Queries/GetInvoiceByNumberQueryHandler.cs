using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.Billing.Application.Features.Invoicing.Mappers;
using EHRPlatform.Services.Billing.Application.Features.Invoicing.Queries;
using EHRPlatform.Services.Billing.Contracts.Responses;
using EHRPlatform.Services.Billing.Persistence;
using Microsoft.EntityFrameworkCore;

namespace EHRPlatform.Services.Billing.Application.Features.Invoicing.Handlers;

/// <summary>
/// Handler for retrieving an invoice by its number.
/// </summary>
public class GetInvoiceByNumberQueryHandler : IRequestHandler<GetInvoiceByNumberQuery, InvoiceResponseDto?>
{
    private readonly BillingContext _context;
    private readonly InvoiceMapper _mapper;
    private readonly ILogger<GetInvoiceByNumberQueryHandler> _logger;

    public GetInvoiceByNumberQueryHandler(
        BillingContext context,
        InvoiceMapper mapper,
        ILogger<GetInvoiceByNumberQueryHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger;
    }

    public async Task<InvoiceResponseDto?> Handle(
        GetInvoiceByNumberQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving invoice {InvoiceNumber}", request.InvoiceNumber);

        var invoice = await _context.Invoices
            .Include(x => x.LineItems)
            .Include(x => x.Payments)
            .Include(x => x.InsuranceClaims)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.InvoiceNumber == request.InvoiceNumber, cancellationToken);

        if (invoice == null)
        {
            _logger.LogWarning("Invoice not found: {InvoiceNumber}", request.InvoiceNumber);
            return null;
        }

        return _mapper.MapToResponseDto(invoice);
    }
}
