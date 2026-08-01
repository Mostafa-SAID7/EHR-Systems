namespace EHRPlatform.Services.Integration.Application.Features.HL7.Commands;

using MediatR;
using EHRPlatform.Services.Integration.Domain.Entities;
using EHRPlatform.Services.Integration.Persistence;
using EHRPlatform.Services.Integration.Application.Services;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for ReceiveHL7MessageCommand - Receives and parses HL7 messages.
/// </summary>
public class ReceiveHL7MessageCommandHandler : IRequestHandler<ReceiveHL7MessageCommand, ReceiveHL7MessageResponse>
{
    private readonly IIntegrationDbContext _context;
    private readonly IHL7ParserService _hl7ParserService;
    private readonly ILogger<ReceiveHL7MessageCommandHandler> _logger;

    public ReceiveHL7MessageCommandHandler(
        IIntegrationDbContext context,
        IHL7ParserService hl7ParserService,
        ILogger<ReceiveHL7MessageCommandHandler> logger)
    {
        _context = context;
        _hl7ParserService = hl7ParserService;
        _logger = logger;
    }

    public async Task<ReceiveHL7MessageResponse> Handle(ReceiveHL7MessageCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Receiving HL7 message from {SendingApp}", request.SendingApplication);

        // Parse HL7 message
        var parseResult = await _hl7ParserService.ParseHL7Async(request.HL7Content, cancellationToken);

        if (!parseResult.IsValid)
        {
            throw new InvalidOperationException($"HL7 parsing failed: {string.Join(", ", parseResult.Errors)}");
        }

        // Create message entity
        var message = new HL7Message
        {
            Id = Guid.NewGuid(),
            HL7Content = request.HL7Content,
            MessageId = parseResult.MessageId,
            MessageType = parseResult.MessageType,
            EventType = parseResult.EventType,
            SegmentType = parseResult.SegmentType,
            PatientId = parseResult.PatientId,
            EncounterId = parseResult.EncounterId,
            MessageDateTime = parseResult.MessageDateTime,
            SendingApplication = request.SendingApplication,
            ReceivingApplication = request.ReceivingApplication,
            Status = "Parsed",
            CreatedAt = DateTime.UtcNow
        };

        _context.HL7Messages.Add(message);

        // Store parsed segments
        for (int i = 0; i < parseResult.Segments.Count; i++)
        {
            var segment = parseResult.Segments[i];
            var messagePart = new HL7MessagePart
            {
                Id = Guid.NewGuid(),
                HL7MessageId = message.Id,
                SegmentId = segment.Key,
                SegmentContent = segment.Value,
                SequenceNumber = i,
                CreatedAt = DateTime.UtcNow
            };
            _context.HL7MessageParts.Add(messagePart);
        }

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("HL7 message received: {MessageId} (Type: {MessageType})", 
            message.Id, parseResult.MessageType);

        return new ReceiveHL7MessageResponse
        {
            MessageId = message.Id,
            Received = true,
            MessageType = parseResult.MessageType,
            PatientId = parseResult.PatientId?.ToString(),
            EncounterId = parseResult.EncounterId?.ToString(),
            ReceivedAt = DateTime.UtcNow
        };
    }
}
