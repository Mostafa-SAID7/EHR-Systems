using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.Services.Billing.Features.Invoicing.Queries;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Billing.Features.Invoicing.Handlers;

/// <summary>
/// Get invoice by ID handler.
/// Pure business logic - no mapping responsibility.
/// </summary>
public class GetInvoiceQueryHandler : IQueryHandler<GetInvoiceQuery, InvoiceResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly InvoiceMapper _mapper;
    private readonly ILogger<GetInvoiceQueryHandler> _logger;

    public GetInvoiceQueryHandler(
        IUnitOfWork unitOfWork,
        InvoiceMapper mapper,
        ILogger<GetInvoiceQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger;
    }

    public async Task<InvoiceResponseDto> Handle(
        GetInvoiceQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching invoice {InvoiceId}", request.InvoiceId);

        var repo = _unitOfWork.Repository<Invoice>();
        var invoice = await repo.FirstOrDefaultAsync(
            q => q.Where(i => i.Id == request.InvoiceId),
            cancellationToken);

        if (invoice == null)
            throw new InvalidOperationException($"Invoice {request.InvoiceId} not found");

        // Delegate mapping to mapper - clean separation of concerns
        return _mapper.MapToResponseDto(invoice);
    }
}


