namespace EHRPlatform.Services.FileStorage.Application.Features.Documents.Queries;

using MediatR;
using EHRPlatform.Services.FileStorage.Persistence;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for GetDocumentRetentionStatusQuery - Retrieves retention info.
/// Calculates days until automatic deletion.
/// </summary>
public class GetDocumentRetentionStatusQueryHandler : IRequestHandler<GetDocumentRetentionStatusQuery, DocumentRetentionStatusDto>
{
    private readonly IFileStorageDbContext _context;
    private readonly ILogger<GetDocumentRetentionStatusQueryHandler> _logger;

    public GetDocumentRetentionStatusQueryHandler(
        IFileStorageDbContext context,
        ILogger<GetDocumentRetentionStatusQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<DocumentRetentionStatusDto> Handle(GetDocumentRetentionStatusQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving retention status for document {DocumentId}", request.DocumentId);

        var document = await _context.StoredDocuments.FindAsync(new object[] { request.DocumentId }, cancellationToken);
        if (document == null)
        {
            throw new InvalidOperationException($"Document {request.DocumentId} not found");
        }

        var daysUntilDeletion = 0;
        if (document.ScheduledDeletionDate.HasValue)
        {
            daysUntilDeletion = (int)(document.ScheduledDeletionDate.Value - DateTime.UtcNow).TotalDays;
            daysUntilDeletion = Math.Max(0, daysUntilDeletion);
        }

        return new DocumentRetentionStatusDto
        {
            DocumentId = document.Id,
            RetentionPolicyId = document.RetentionPolicyId,
            IsMarkedForDeletion = document.IsMarkedForDeletion,
            ScheduledDeletionDate = document.ScheduledDeletionDate,
            DaysUntilDeletion = daysUntilDeletion,
            Status = document.Status
        };
    }
}
