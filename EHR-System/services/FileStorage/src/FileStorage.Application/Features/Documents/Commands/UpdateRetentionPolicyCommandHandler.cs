namespace EHRPlatform.Services.FileStorage.Application.Features.Documents.Commands;

using MediatR;
using EHRPlatform.Services.FileStorage.Domain.Entities;
using EHRPlatform.Services.FileStorage.Persistence;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for UpdateRetentionPolicyCommand - Updates document retention.
/// Applies retention policy and calculates expiration date.
/// </summary>
public class UpdateRetentionPolicyCommandHandler : IRequestHandler<UpdateRetentionPolicyCommand, UpdateRetentionPolicyResponse>
{
    private readonly IFileStorageDbContext _context;
    private readonly ILogger<UpdateRetentionPolicyCommandHandler> _logger;

    public UpdateRetentionPolicyCommandHandler(
        IFileStorageDbContext context,
        ILogger<UpdateRetentionPolicyCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<UpdateRetentionPolicyResponse> Handle(UpdateRetentionPolicyCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating retention policy for document {DocumentId} to policy {PolicyId}", 
            request.DocumentId, request.RetentionPolicyId);

        var document = await _context.StoredDocuments.FindAsync(new object[] { request.DocumentId }, cancellationToken);
        if (document == null)
        {
            throw new InvalidOperationException($"Document {request.DocumentId} not found");
        }

        document.RetentionPolicyId = request.RetentionPolicyId;
        document.UpdatedAt = DateTime.UtcNow;

        // Calculate expiration date based on retention days or custom date
        var expirationDate = request.CustomExpirationDate ?? DateTime.UtcNow.AddDays(request.RetentionDays);

        // Store expiration info (could be extended with a separate RetentionSchedule entity)
        document.ScheduledDeletionDate = expirationDate;

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Retention policy updated for document {DocumentId}. Expiration: {ExpirationDate}", 
            request.DocumentId, expirationDate);

        return new UpdateRetentionPolicyResponse
        {
            DocumentId = document.Id,
            RetentionPolicyId = request.RetentionPolicyId,
            RetentionDays = request.RetentionDays,
            ExpirationDate = expirationDate,
            Updated = true
        };
    }
}
