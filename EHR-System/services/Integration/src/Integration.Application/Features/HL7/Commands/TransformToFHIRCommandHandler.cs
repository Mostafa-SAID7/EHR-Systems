namespace EHRPlatform.Services.Integration.Application.Features.HL7.Commands;

using MediatR;
using EHRPlatform.Services.Integration.Domain.Entities;
using EHRPlatform.Services.Integration.Persistence;
using EHRPlatform.Services.Integration.Application.Services;
using Microsoft.Extensions.Logging;

/// <summary>
/// Handler for TransformToFHIRCommand - Transforms HL7 to FHIR.
/// </summary>
public class TransformToFHIRCommandHandler : IRequestHandler<TransformToFHIRCommand, TransformToFHIRResponse>
{
    private readonly IIntegrationDbContext _context;
    private readonly IFHIRTransformerService _fhirTransformerService;
    private readonly IFHIRValidationService _fhirValidationService;
    private readonly ILogger<TransformToFHIRCommandHandler> _logger;

    public TransformToFHIRCommandHandler(
        IIntegrationDbContext context,
        IFHIRTransformerService fhirTransformerService,
        IFHIRValidationService fhirValidationService,
        ILogger<TransformToFHIRCommandHandler> logger)
    {
        _context = context;
        _fhirTransformerService = fhirTransformerService;
        _fhirValidationService = fhirValidationService;
        _logger = logger;
    }

    public async Task<TransformToFHIRResponse> Handle(TransformToFHIRCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Transforming HL7 message {MessageId} to FHIR", request.HL7MessageId);

        var message = await _context.HL7Messages.FindAsync(new object[] { request.HL7MessageId }, cancellationToken);
        if (message == null)
        {
            throw new InvalidOperationException($"HL7 message {request.HL7MessageId} not found");
        }

        // Transform to FHIR
        var fhirResult = await _fhirTransformerService.TransformHL7ToFHIRAsync(message, cancellationToken);

        // Validate FHIR
        var validationResult = await _fhirValidationService.ValidateFHIRAsync(fhirResult.FHIRContent, cancellationToken);

        // Store transformation
        var transformation = new FHIRTransformation
        {
            Id = Guid.NewGuid(),
            HL7MessageId = message.Id,
            ResourceType = fhirResult.ResourceType,
            FHIRContent = fhirResult.FHIRContent,
            IsValid = validationResult.IsValid,
            ValidationErrors = validationResult.IsValid ? null : string.Join("; ", validationResult.Errors),
            CreatedAt = DateTime.UtcNow
        };

        _context.FHIRTransformations.Add(transformation);
        message.Status = "Transformed";
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("HL7 message transformed to FHIR {ResourceType}", fhirResult.ResourceType);

        return new TransformToFHIRResponse
        {
            HL7MessageId = message.Id,
            FHIRTransformationId = transformation.Id,
            ResourceType = fhirResult.ResourceType,
            IsValid = validationResult.IsValid,
            FHIRContent = fhirResult.FHIRContent,
            ValidationErrors = validationResult.Errors
        };
    }
}
