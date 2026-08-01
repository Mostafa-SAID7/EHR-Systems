using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Observability.Logging;

/// <summary>
/// Implementation of structured logger.
/// Single responsibility: JSON-based structured logging.
/// </summary>
public class StructuredLoggerImpl : IStructuredLogger
{
    private readonly ILogger _logger;

    public StructuredLoggerImpl(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void LogInformation(string message, Dictionary<string, object>? data = null, string? traceId = null)
    {
        var logData = BuildLogData(message, data, traceId);
        _logger.LogInformation("{Message}", System.Text.Json.JsonSerializer.Serialize(logData));
    }

    public void LogWarning(string message, Dictionary<string, object>? data = null, string? traceId = null)
    {
        var logData = BuildLogData(message, data, traceId);
        _logger.LogWarning("{Message}", System.Text.Json.JsonSerializer.Serialize(logData));
    }

    public void LogError(string message, Exception? exception = null, Dictionary<string, object>? data = null, string? traceId = null)
    {
        var logData = BuildLogData(message, data, traceId);
        if (exception != null)
        {
            logData["exception"] = exception.Message;
            logData["stackTrace"] = exception.StackTrace;
        }
        _logger.LogError("{Message}", System.Text.Json.JsonSerializer.Serialize(logData));
    }

    public void LogDebug(string message, Dictionary<string, object>? data = null, string? traceId = null)
    {
        var logData = BuildLogData(message, data, traceId);
        _logger.LogDebug("{Message}", System.Text.Json.JsonSerializer.Serialize(logData));
    }

    public void LogAudit(string action, string resource, string userId, bool success, string? details = null, string? traceId = null)
    {
        var data = new Dictionary<string, object>
        {
            ["action"] = action,
            ["resource"] = resource,
            ["userId"] = userId,
            ["success"] = success,
        };

        if (!string.IsNullOrEmpty(details))
            data["details"] = details;

        var logData = BuildLogData($"Audit: {action}", data, traceId);
        logData["audit"] = true;

        _logger.LogInformation("{Message}", System.Text.Json.JsonSerializer.Serialize(logData));
    }

    private static Dictionary<string, object> BuildLogData(string message, Dictionary<string, object>? data, string? traceId)
    {
        var logData = new Dictionary<string, object>
        {
            ["message"] = message,
            ["timestamp"] = DateTime.UtcNow,
        };

        if (!string.IsNullOrEmpty(traceId))
            logData["traceId"] = traceId;

        if (data != null)
        {
            foreach (var kvp in data)
            {
                logData[kvp.Key] = kvp.Value;
            }
        }

        return logData;
    }
}
