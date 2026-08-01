namespace EHRPlatform.Services.FileStorage.Application.Features.Documents.Queries;

using MediatR;
using EHRPlatform.Services.FileStorage.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for GetVirusScanResultQuery - Retrieves scan result.
/// Returns latest scan for the document.
/// </summary>
public class GetVirusScanResultQueryHandler : IRequestHandler<GetVirusScanResultQuery, VirusScanResultDto>
{
    private readonly IFileStorageDbContext _context;
    private readonly ILogger<GetVirusScanResultQueryHandler> _logger;

    public GetVirusScanResultQueryHandler(
        IFileStorageDbContext context,
        ILogger<GetVirusScanResultQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<VirusScanResultDto> Handle(GetVirusScanResultQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Retrieving virus scan result for document {DocumentId}", request.DocumentId);

        var scanResult = await _context.VirusScanResults
            .Where(s => s.DocumentId == request.DocumentId)
            .OrderByDescending(s => s.ScannedAt)
            .FirstOrDefaultAsync(cancellationToken);

        if (scanResult == null)
        {
            throw new InvalidOperationException($"No scan result found for document {request.DocumentId}");
        }

        return new VirusScanResultDto
        {
            DocumentId = scanResult.DocumentId,
            Result = scanResult.Result,
            ThreatName = scanResult.ThreatName,
            ScannedAt = scanResult.ScannedAt,
            ScanDetails = scanResult.ScanDetails,
            ScannerName = scanResult.ScannerName
        };
    }
}
