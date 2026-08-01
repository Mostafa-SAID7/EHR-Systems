namespace EHRPlatform.Services.Integration.Application.Features.HL7.Queries;

using MediatR;
using EHRPlatform.Services.Integration.Persistence;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for GetHL7MessageStatusQuery - Returns message processing status.
/// </summary>
public class GetHL7MessageStatusQueryHandler : IRequestHandler<GetHL7MessageStatusQuery, HL7MessageStatusDto>
{
    private readonly IIntegrationDbContext _context;
    private readonly ILogger<GetHL7MessageStatusQueryHandler> _logger;

    public GetHL7MessageStatusQueryHandler(
        IIntegrationDbContext context,
        ILogger<GetHL7MessageStatusQueryHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<HL7MessageStatusDto> Handle(GetHL7MessageStatusQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Getting status for HL7 message {MessageId}", request.MessageId);

        var message = await _context.HL7Messages.FindAsync(new object[] { request.MessageId }, cancellationToken);
        if (message == null)
        {
            throw new InvalidOperationException($"HL7 message {request.MessageId} not found");
        }

        return new HL7MessageStatusDto
        {
            MessageId = message.Id,
            Status = message.Status,
            MessageType = message.MessageType,
            PatientId = message.PatientId,
            EncounterId = message.EncounterId,
            IsProcessed = message.IsProcessed,
            ErrorMessage = message.ErrorMessage,
            RetryCount = message.RetryCount,
            CreatedAt = message.CreatedAt,
            UpdatedAt = message.UpdatedAt
        };
    }
}
