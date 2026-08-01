using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Observability.Logging;

/// <summary>
/// Structured logging helper interface for consistent log formatting.
/// Single responsibility: Structured logging contract.
/// </summary>
public interface IStructuredLogger
{
    /// <summary>
    /// Log information with structured data.
    /// </summary>
    void LogInformation(string message, Dictionary<string, object>? data = null, string? traceId = null);

    /// <summary>
    /// Log warning with structured data.
    /// </summary>
    void LogWarning(string message, Dictionary<string, object>? data = null, string? traceId = null);

    /// <summary>
    /// Log error with exception.
    /// </summary>
    void LogError(string message, Exception? exception = null, Dictionary<string, object>? data = null, string? traceId = null);

    /// <summary>
    /// Log debug information.
    /// </summary>
    void LogDebug(string message, Dictionary<string, object>? data = null, string? traceId = null);

    /// <summary>
    /// Log audit event (security-relevant action).
    /// </summary>
    void LogAudit(string action, string resource, string userId, bool success, string? details = null, string? traceId = null);
}

