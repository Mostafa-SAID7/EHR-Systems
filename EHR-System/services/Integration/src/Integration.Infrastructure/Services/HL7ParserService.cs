namespace EHRPlatform.Services.Integration.Infrastructure.Services;

using EHRPlatform.Services.Integration.Application.Services;
using Microsoft.Extensions.Logging;
using System.Text.RegularExpressions;

/// <summary>
/// HL7v2 message parser implementation.
/// Parses HL7 v2.x messages and extracts segments and fields.
/// </summary>
public class HL7ParserService : IHL7ParserService
{
    private readonly ILogger<HL7ParserService> _logger;

    public HL7ParserService(ILogger<HL7ParserService> logger)
    {
        _logger = logger;
    }

    public async Task<HL7ParseResult> ParseHL7Async(string hl7Content, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Parsing HL7 message");

        var result = new HL7ParseResult { IsValid = true };

        try
        {
            var lines = hl7Content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (line.Length < 3) continue;

                var segmentId = line.Substring(0, 3);
                var fields = line.Split('|');

                // Extract key identifiers
                switch (segmentId)
                {
                    case "MSH": // Message Header
                        result.MessageId = fields.Length > 10 ? fields[10] : "";
                        result.MessageType = fields.Length > 9 ? fields[9] : "";
                        result.MessageDateTime = ParseHL7Date(fields.Length > 7 ? fields[7] : "");
                        break;

                    case "PID": // Patient Identification
                        result.PatientId = Guid.TryParse(fields.Length > 3 ? fields[3] : "", out var pid) ? pid : null;
                        break;

                    case "PV1": // Patient Visit
                        result.EncounterId = Guid.TryParse(fields.Length > 19 ? fields[19] : "", out var enc) ? enc : null;
                        break;

                    case "ORC": // Order Common
                    case "OBR": // Observation Request
                    case "OBX": // Observation/Result
                        // Lab result segment
                        break;
                }

                result.Segments[segmentId] = line;
            }

            if (string.IsNullOrEmpty(result.MessageId))
            {
                result.IsValid = false;
                result.Errors.Add("Missing MSH segment or MessageId");
            }

            _logger.LogInformation("HL7 parsing completed. MessageId: {MessageId}, Type: {MessageType}", 
                result.MessageId, result.MessageType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing HL7 message");
            result.IsValid = false;
            result.Errors.Add($"Parse error: {ex.Message}");
        }

        return await Task.FromResult(result);
    }

    public async Task<HL7ValidationResult> ValidateHL7Async(string hl7Content, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Validating HL7 message");

        var result = new HL7ValidationResult { IsValid = true };

        try
        {
            if (string.IsNullOrEmpty(hl7Content))
            {
                result.IsValid = false;
                result.Errors.Add("HL7 content is empty");
                return result;
            }

            if (!hl7Content.StartsWith("MSH"))
            {
                result.IsValid = false;
                result.Errors.Add("HL7 message must start with MSH segment");
                return result;
            }

            var lines = hl7Content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var line in lines)
            {
                if (line.Length < 3)
                {
                    result.Warnings.Add($"Invalid segment length: {line}");
                    continue;
                }

                var segmentId = line.Substring(0, 3);
                if (!Regex.IsMatch(segmentId, @"^[A-Z]{3}$"))
                {
                    result.Warnings.Add($"Invalid segment ID: {segmentId}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error validating HL7 message");
            result.IsValid = false;
            result.Errors.Add($"Validation error: {ex.Message}");
        }

        return await Task.FromResult(result);
    }

    private DateTime ParseHL7Date(string hl7Date)
    {
        if (string.IsNullOrEmpty(hl7Date) || hl7Date.Length < 8)
            return DateTime.UtcNow;

        if (DateTime.TryParseExact(hl7Date, "yyyyMMdd", null, System.Globalization.DateTimeStyles.None, out var date))
            return date;

        if (DateTime.TryParseExact(hl7Date, "yyyyMMddHHmmss", null, System.Globalization.DateTimeStyles.None, out var dateTime))
            return dateTime;

        return DateTime.UtcNow;
    }
}
