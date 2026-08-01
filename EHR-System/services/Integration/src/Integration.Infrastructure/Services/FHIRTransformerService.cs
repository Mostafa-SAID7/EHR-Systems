namespace EHRPlatform.Services.Integration.Infrastructure.Services;

using EHRPlatform.Services.Integration.Application.Services;
using EHRPlatform.Services.Integration.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Text.Json;

/// <summary>
/// HL7 to FHIR transformation service implementation.
/// Converts HL7v2 messages to FHIR R4 JSON resources.
/// </summary>
public class FHIRTransformerService : IFHIRTransformerService
{
    private readonly ILogger<FHIRTransformerService> _logger;

    public FHIRTransformerService(ILogger<FHIRTransformerService> logger)
    {
        _logger = logger;
    }

    public async Task<FHIRTransformResult> TransformHL7ToFHIRAsync(HL7Message message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Transforming HL7 message {MessageId} to FHIR", message.Id);

        try
        {
            var result = new FHIRTransformResult();

            // Determine resource type based on message type
            result.ResourceType = message.MessageType switch
            {
                "ADT^A01" => "Patient",
                "ORU^R01" => "DiagnosticReport",
                "ORM^O01" => "ServiceRequest",
                "RGV^O15" => "MedicationRequest",
                _ => "Bundle"
            };

            // Create FHIR resource as JSON
            var fhirResource = new
            {
                resourceType = result.ResourceType,
                id = message.Id.ToString(),
                meta = new
                {
                    profile = new[] { $"http://hl7.org/fhir/StructureDefinition/{result.ResourceType}" },
                    lastUpdated = DateTime.UtcNow.ToString("o")
                },
                identifier = new object[]
                {
                    new
                    {
                        system = "http://example.com/hl7-message-id",
                        value = message.MessageId
                    }
                },
                // Additional fields based on message content
                status = "final",
                subject = message.PatientId.HasValue ? new { reference = $"Patient/{message.PatientId}" } : null
            };

            result.FHIRContent = JsonSerializer.Serialize(fhirResource, new JsonSerializerOptions { WriteIndented = true });

            _logger.LogInformation("HL7 transformed to FHIR resource: {ResourceType}", result.ResourceType);

            return await Task.FromResult(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transforming HL7 to FHIR");
            throw;
        }
    }

    public async Task<string> TransformSegmentToFHIRAsync(string segmentId, string segmentContent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Transforming segment {SegmentId} to FHIR", segmentId);

        try
        {
            var fhirSegment = segmentId switch
            {
                "PID" => TransformPIDSegment(segmentContent),
                "OBX" => TransformOBXSegment(segmentContent),
                "OBR" => TransformOBRSegment(segmentContent),
                _ => JsonSerializer.Serialize(new { segment = segmentId, content = segmentContent })
            };

            return await Task.FromResult(fhirSegment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error transforming segment");
            throw;
        }
    }

    private string TransformPIDSegment(string content)
    {
        var fields = content.Split('|');
        var resource = new
        {
            resourceType = "Patient",
            name = new object[]
            {
                new
                {
                    use = "official",
                    given = new[] { fields.Length > 6 ? fields[6] : "" },
                    family = fields.Length > 5 ? fields[5] : ""
                }
            },
            birthDate = fields.Length > 7 ? fields[7] : ""
        };

        return JsonSerializer.Serialize(resource);
    }

    private string TransformOBXSegment(string content)
    {
        var fields = content.Split('|');
        var resource = new
        {
            resourceType = "Observation",
            code = new
            {
                coding = new object[]
                {
                    new
                    {
                        system = "http://loinc.org",
                        code = fields.Length > 3 ? fields[3] : ""
                    }
                }
            },
            valueString = fields.Length > 5 ? fields[5] : ""
        };

        return JsonSerializer.Serialize(resource);
    }

    private string TransformOBRSegment(string content)
    {
        var fields = content.Split('|');
        var resource = new
        {
            resourceType = "DiagnosticReport",
            code = new
            {
                coding = new object[]
                {
                    new
                    {
                        system = "http://loinc.org",
                        code = fields.Length > 4 ? fields[4] : ""
                    }
                }
            },
            status = "final"
        };

        return JsonSerializer.Serialize(resource);
    }
}
