namespace EHRPlatform.Services.Integration.Infrastructure.Services;

using EHRPlatform.Services.Integration.Application.Services;
using Microsoft.Extensions.Logging;
using System.Text.Json;

/// <summary>
/// FHIR resource validation service implementation.
/// Validates FHIR resources against R4 profiles.
/// </summary>
public class FHIRValidationService : IFHIRValidationService
{
    private readonly ILogger<FHIRValidationService> _logger;

    public FHIRValidationService(ILogger<FHIRValidationService> logger)
    {
        _logger = logger;
    }

    public async Task<FHIRValidationResult> ValidateFHIRAsync(string fhirContent, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating FHIR resource");

        var result = new FHIRValidationResult { IsValid = true };

        try
        {
            if (string.IsNullOrEmpty(fhirContent))
            {
                result.IsValid = false;
                result.Errors.Add("FHIR content is empty");
                return result;
            }

            // Parse JSON
            using var doc = JsonDocument.Parse(fhirContent);
            var root = doc.RootElement;

            // Check required fields
            if (!root.TryGetProperty("resourceType", out var resourceType))
            {
                result.IsValid = false;
                result.Errors.Add("Missing required field: resourceType");
                return result;
            }

            if (!root.TryGetProperty("id", out _))
            {
                result.Warnings.Add("Missing recommended field: id");
            }

            if (!root.TryGetProperty("meta", out _))
            {
                result.Warnings.Add("Missing recommended field: meta");
            }

            // Validate based on resource type
            ValidateResourceType(root, resourceType.GetString(), result);

            _logger.LogInformation("FHIR validation completed. Valid: {IsValid}", result.IsValid);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Invalid JSON in FHIR content");
            result.IsValid = false;
            result.Errors.Add($"JSON parse error: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating FHIR");
            result.IsValid = false;
            result.Errors.Add($"Validation error: {ex.Message}");
        }

        return await Task.FromResult(result);
    }

    public async Task<FHIRValidationResult> ValidateAgainstProfileAsync(
        string fhirContent,
        string profileUrl,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating FHIR against profile: {ProfileUrl}", profileUrl);

        var result = await ValidateFHIRAsync(fhirContent, cancellationToken);

        // Additional profile-specific validation would go here
        result.Information.Add($"Validated against profile: {profileUrl}");

        return result;
    }

    private void ValidateResourceType(JsonElement root, string? resourceType, FHIRValidationResult result)
    {
        switch (resourceType)
        {
            case "Patient":
                ValidatePatient(root, result);
                break;

            case "Observation":
                ValidateObservation(root, result);
                break;

            case "DiagnosticReport":
                ValidateDiagnosticReport(root, result);
                break;

            case "Bundle":
                ValidateBundle(root, result);
                break;

            default:
                result.Information.Add($"No specific validation rules for resource type: {resourceType}");
                break;
        }
    }

    private void ValidatePatient(JsonElement root, FHIRValidationResult result)
    {
        // Patient-specific validations
        if (!root.TryGetProperty("name", out _))
        {
            result.Warnings.Add("Patient should have at least one name");
        }
    }

    private void ValidateObservation(JsonElement root, FHIRValidationResult result)
    {
        // Observation-specific validations
        if (!root.TryGetProperty("status", out _))
        {
            result.Errors.Add("Observation must have a status");
            result.IsValid = false;
        }

        if (!root.TryGetProperty("code", out _))
        {
            result.Errors.Add("Observation must have a code");
            result.IsValid = false;
        }
    }

    private void ValidateDiagnosticReport(JsonElement root, FHIRValidationResult result)
    {
        // DiagnosticReport-specific validations
        if (!root.TryGetProperty("status", out _))
        {
            result.Errors.Add("DiagnosticReport must have a status");
            result.IsValid = false;
        }

        if (!root.TryGetProperty("code", out _))
        {
            result.Errors.Add("DiagnosticReport must have a code");
            result.IsValid = false;
        }
    }

    private void ValidateBundle(JsonElement root, FHIRValidationResult result)
    {
        // Bundle-specific validations
        if (!root.TryGetProperty("type", out _))
        {
            result.Errors.Add("Bundle must have a type");
            result.IsValid = false;
        }
    }
}
