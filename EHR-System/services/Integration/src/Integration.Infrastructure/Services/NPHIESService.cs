namespace EHRPlatform.Services.Integration.Infrastructure.Services;

using EHRPlatform.Services.Integration.Application.Services;
using EHRPlatform.Services.Integration.Domain.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;

/// <summary>
/// NPHIES (National Program for Health Insurance) integration service.
/// Submits claims to NPHIES and retrieves claim status.
/// </summary>
public class NPHIESService : INPHIESService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<NPHIESService> _logger;
    private readonly IConfiguration _configuration;

    public NPHIESService(
        HttpClient httpClient,
        ILogger<NPHIESService> logger,
        IConfiguration configuration)
    {
        _httpClient = httpClient;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<NPHIESSubmissionResult> SubmitClaimAsync(
        HL7Message message,
        string claimType,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Submitting claim to NPHIES for message {MessageId}", message.Id);

        try
        {
            // Get NPHIES API endpoint from config
            var nphiesEndpoint = _configuration["NPHIES:Endpoint"] ?? "https://api.nphies.sa/claims";
            var apiKey = _configuration["NPHIES:ApiKey"];

            // Convert message to NPHIES format
            var nphiesPayload = await ConvertToNPHIESFormatAsync(message.HL7Content, cancellationToken);

            // Prepare request
            var request = new HttpRequestMessage(HttpMethod.Post, $"{nphiesEndpoint}/submit")
            {
                Content = new StringContent(nphiesPayload, System.Text.Encoding.UTF8, "application/json")
            };

            request.Headers.Add("Authorization", $"Bearer {apiKey}");
            request.Headers.Add("X-Claim-Type", claimType);

            // Submit claim
            var response = await _httpClient.SendAsync(request, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("NPHIES submission failed with status {StatusCode}", response.StatusCode);
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

            // Parse response
            var claimNumber = ExtractClaimNumber(responseContent);

            _logger.LogInformation("Claim submitted to NPHIES: {ClaimNumber}", claimNumber);

            return new NPHIESSubmissionResult
            {
                ClaimNumber = claimNumber,
                Response = responseContent,
                TotalAmount = ExtractTotalAmount(responseContent),
                IsAccepted = response.IsSuccessStatusCode
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting claim to NPHIES");
            throw;
        }
    }

    public async Task<NPHIESClaimStatus> GetClaimStatusAsync(
        string claimNumber,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting NPHIES claim status: {ClaimNumber}", claimNumber);

        try
        {
            var nphiesEndpoint = _configuration["NPHIES:Endpoint"] ?? "https://api.nphies.sa/claims";
            var apiKey = _configuration["NPHIES:ApiKey"];

            var request = new HttpRequestMessage(HttpMethod.Get, $"{nphiesEndpoint}/claims/{claimNumber}")
            {
            };

            request.Headers.Add("Authorization", $"Bearer {apiKey}");

            var response = await _httpClient.SendAsync(request, cancellationToken);
            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            return new NPHIESClaimStatus
            {
                ClaimNumber = claimNumber,
                Status = ExtractStatus(content),
                TotalAmount = ExtractTotalAmount(content),
                PaidAmount = ExtractPaidAmount(content)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting NPHIES claim status");
            throw;
        }
    }

    public async Task<string> ConvertToNPHIESFormatAsync(
        string fhirContent,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Converting to NPHIES format");

        // NPHIES uses its own claim format - this is a simplified conversion
        // In production: use FHIR-to-NPHIES mapping library

        var nphiesClaim = new
        {
            apiVersion = "1.0",
            resourceType = "ClaimRequest",
            claim = new
            {
                organizationId = _configuration["NPHIES:OrganizationId"],
                claimType = "professional",
                claimItems = new object[] { }
            }
        };

        return await Task.FromResult(System.Text.Json.JsonSerializer.Serialize(nphiesClaim));
    }

    private string ExtractClaimNumber(string response)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(response);
            if (doc.RootElement.TryGetProperty("claimNumber", out var claimNumber))
            {
                return claimNumber.GetString() ?? $"CLAIM-{Guid.NewGuid():N}";
            }
        }
        catch { }

        return $"CLAIM-{Guid.NewGuid():N}";
    }

    private string ExtractStatus(string response)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(response);
            if (doc.RootElement.TryGetProperty("status", out var status))
            {
                return status.GetString() ?? "Unknown";
            }
        }
        catch { }

        return "Unknown";
    }

    private decimal ExtractTotalAmount(string response)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(response);
            if (doc.RootElement.TryGetProperty("totalAmount", out var amount))
            {
                return amount.GetDecimal();
            }
        }
        catch { }

        return 0;
    }

    private decimal ExtractPaidAmount(string response)
    {
        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(response);
            if (doc.RootElement.TryGetProperty("paidAmount", out var amount))
            {
                return amount.GetDecimal();
            }
        }
        catch { }

        return 0;
    }
}
