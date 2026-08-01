using MediatR;
using Microsoft.Extensions.Logging;
using EHRPlatform.Services.FileStorage.Application.Features.Documents.Mappers;
using EHRPlatform.Services.FileStorage.Application.Features.Documents.Queries;
using EHRPlatform.Services.FileStorage.Contracts.Responses;
using EHRPlatform.Services.FileStorage.Persistence;

namespace EHRPlatform.Services.FileStorage.Application.Features.Documents.Handlers;

/// <summary>
/// Handler for retrieving a document.
/// </summary>
public class GetDocumentQueryHandler : IRequestHandler<GetDocumentQuery, DocumentResponseDto?>
{
    private readonly FileStorageContext _context;
    private readonly DocumentMapper _mapper;
    private readonly ILogger<GetDocumentQueryHandler> _logger;

    public GetDocumentQueryHandler(
        FileStorageContext context,
        DocumentMapper mapper,
        ILogger<GetDocumentQueryHandler> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger;
    }

    public async Task<DocumentResponseDto?> Handle(
        GetDocumentQuery request,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving document {DocumentId}", request.DocumentId);

        var document = await _context.StoredDocuments
            .FindAsync(new object[] { request.DocumentId }, cancellationToken: cancellationToken);

        if (document == null)
        {
            _logger.LogWarning("Document not found: {DocumentId}", request.DocumentId);
            return null;
        }

        return _mapper.MapToResponseDto(document);
    }
}
