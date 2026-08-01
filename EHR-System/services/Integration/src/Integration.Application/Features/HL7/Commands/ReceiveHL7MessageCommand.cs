namespace EHRPlatform.Services.Integration.Application.Features.HL7.Commands;

using MediatR;

/// <summary>
/// Command to receive and parse an HL7 message.
/// Validates format, extracts patient/encounter, stores for processing.
/// </summary>
public class ReceiveHL7MessageCommand : IRequest<ReceiveHL7MessageResponse>
{
    public string HL7Content { get; set; } = string.Empty;
    public string? SendingApplication { get; set; }
    public string? ReceivingApplication { get; set; }
}

public class ReceiveHL7MessageResponse
{
    public Guid MessageId { get; set; }
    public bool Received { get; set; }
    public string MessageType { get; set; } = string.Empty;
    public string? PatientId { get; set; }
    public string? EncounterId { get; set; }
    public DateTime ReceivedAt { get; set; }
}
