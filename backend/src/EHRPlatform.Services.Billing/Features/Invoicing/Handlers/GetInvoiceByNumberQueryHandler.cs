using EHRPlatform.BuildingBlocks.Common.Application.CQRS;
using EHRPlatform.BuildingBlocks.Common.Data.Abstractions;
using EHRPlatform.BuildingBlocks.Common.Slugs;
using EHRPlatform.Services.Billing.Features.Invoicing.Queries;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.Billing.Features.Invoicing.Handlers;

/// <summary>
/// Get invoice by InvoiceNumber handler.
/// Automatically cached by CachingBehavior.
/// Generates InvoiceNumberSlug for response.
/// </summary>
public class GetInvoiceByNumberQueryHandler : IQueryHandler<GetInvoiceByNumberQuery, InvoiceResponseDto>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly InvoiceMapper _mapper;
    private readonly ISlugGenerator _slugGenerator;
    private readonly ILogger<GetInvoiceByNumberQueryHandler> _logger;

    public GetInvoiceByNumberQueryHandler(
        IUnitOfWork unitOfWork,
        InvoiceMapper mapper,
        ISlugGenerator slugGenerator,
        ILogger<GetInvoiceByNumberQueryHandler> logger)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _slugGenerator = slugGenerator ?? throw new ArgumentNullException(nameof(slugGenerator));
        _logger = logger;
    }

    public async Task<InvoiceResponseDto> Handle(
        GetInvoiceByNumberQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching invoice by number {InvoiceNumber}", request.InvoiceNumber);

        var repo = _unitOfWork.Repository<Invoice>();
        var invoice = await repo.FirstOrDefaultAsync(
            q => q.Where(i => i.InvoiceNumber == request.InvoiceNumber),
            cancellationToken);

        if (invoice == null)
            throw new InvalidOperationException($"Invoice {request.InvoiceNumber} not found");

        // Delegate mapping to mapper
        var dto = _mapper.MapToResponseDto(invoice);
        
        // Generate slug for URL-friendly access
        dto.InvoiceNumberSlug = _slugGenerator.Generate(invoice.InvoiceNumber);
        dto.Slug = dto.InvoiceNumberSlug;
        dto.SlugDisplayName = invoice.InvoiceNumber;

        return dto;
    }
}


