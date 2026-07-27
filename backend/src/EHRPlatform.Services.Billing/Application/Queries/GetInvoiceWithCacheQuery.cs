using EHRPlatform.Common.CQRS;
using EHRPlatform.Services.Billing.Application.Services;
using EHRPlatform.Services.Billing.Data;
using Microsoft.EntityFrameworkCore;

namespace EHRPlatform.Services.Billing.Application.Queries;

/// <summary>
/// Get invoice by ID with Redis caching.
/// Demonstrates GetOrSetAsync pattern (prevents thundering herd).
/// </summary>
public record GetInvoiceWithCacheQuery(Guid InvoiceId) : IQuery<InvoiceDto>;

public class GetInvoiceWithCacheQueryHandler : IQueryHandler<GetInvoiceWithCacheQuery, InvoiceDto>
{
    private readonly BillingContext _context;
    private readonly IBillingCacheService _cacheService;
    private readonly ILogger<GetInvoiceWithCacheQueryHandler> _logger;

    public GetInvoiceWithCacheQueryHandler(
        BillingContext context,
        IBillingCacheService cacheService,
        ILogger<GetInvoiceWithCacheQueryHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _cacheService = cacheService ?? throw new ArgumentNullException(nameof(cacheService));
        _logger = logger;
    }

    public async Task<InvoiceDto> Handle(GetInvoiceWithCacheQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting invoice: {InvoiceId}", request.InvoiceId);

        // GetOrSetAsync prevents thundering herd:
        // - If cached: returns instantly
        // - If not cached: calls loader once, caches result, all requests wait for same result
        var invoice = await _cacheService.GetOrSetAsync(
            $"billing:invoice:{request.InvoiceId}",
            async (key) =>
            {
                _logger.LogInformation("Loading invoice from database: {InvoiceId}", request.InvoiceId);
                
                var entity = await _context.Invoices
                    .Include(i => i.LineItems)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(i => i.Id == request.InvoiceId, cancellationToken);

                if (entity == null)
                {
                    _logger.LogWarning("Invoice not found: {InvoiceId}", request.InvoiceId);
                    throw new KeyNotFoundException($"Invoice {request.InvoiceId} not found");
                }

                return MapToDto(entity);
            },
            expiry: TimeSpan.FromHours(1));

        return invoice ?? throw new InvalidOperationException("Invoice could not be loaded");
    }

    private static InvoiceDto MapToDto(Invoice entity)
    {
        return new InvoiceDto(
            entity.Id,
            entity.InvoiceNumber,
            entity.PatientId,
            entity.TotalAmount,
            entity.Status,
            entity.CreatedAt,
            entity.LineItems.Select(li => new LineItemDto(
                li.Id,
                li.Description,
                li.Quantity,
                li.UnitPrice,
                li.Amount
            )).ToList()
        );
    }
}

public record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    Guid PatientId,
    decimal Amount,
    string Status,
    DateTime CreatedAt,
    IReadOnlyList<LineItemDto> LineItems);

public record LineItemDto(
    Guid Id,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal Amount);
