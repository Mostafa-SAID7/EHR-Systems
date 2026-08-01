namespace EHRPlatform.Services.Audit.Application.Features.Audit.Commands;

using MediatR;
using EHRPlatform.Services.Audit.Domain.Entities;
using EHRPlatform.Services.Audit.Persistence;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for RecordAuditEntryCommand - Creates immutable audit entry.
/// </summary>
public class RecordAuditEntryCommandHandler : IRequestHandler<RecordAuditEntryCommand, RecordAuditEntryResponse>
{
    private readonly IAuditDbContext _context;
    private readonly ILogger<RecordAuditEntryCommandHandler> _logger;

    public RecordAuditEntryCommandHandler(
        IAuditDbContext context,
        ILogger<RecordAuditEntryCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<RecordAuditEntryResponse> Handle(RecordAuditEntryCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Recording audit entry: {Action} on {ResourceType}/{ResourceId} by {UserEmail}", 
            request.Action, request.ResourceType, request.ResourceId, request.UserEmail);

        try
        {
            // Create immutable audit entry
            var auditEntry = new AuditEntry(
                request.UserId,
                request.UserEmail,
                request.UserFullName,
                request.Action,
                request.ResourceType,
                request.ResourceId,
                request.IpAddress,
                request.UserAgent,
                request.HttpMethod,
                request.Endpoint);

            // Set optional fields
            auditEntry.SetPiiFlags(
                request.ContainsSsn,
                request.ContainsDob,
                request.ContainsMrn,
                request.ContainsPhoneNumber);

            auditEntry.SetAccessLevel(request.AccessLevel);
            auditEntry.SetChangeDetails(request.ChangeDetails);
            
            if (!string.IsNullOrEmpty(request.ErrorMessage))
            {
                auditEntry.SetStatus("Failed");
                auditEntry.SetErrorMessage(request.ErrorMessage);
            }

            // Calculate integrity hash
            auditEntry.CalculateIntegrityHash();

            _context.AuditEntries.Add(auditEntry);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Audit entry recorded: {AuditId}", auditEntry.Id);

            return new RecordAuditEntryResponse
            {
                Success = true,
                AuditId = auditEntry.Id,
                Message = "Audit entry recorded successfully"
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording audit entry");
            return new RecordAuditEntryResponse
            {
                Success = false,
                Message = "An error occurred while recording the audit entry"
            };
        }
    }
}
