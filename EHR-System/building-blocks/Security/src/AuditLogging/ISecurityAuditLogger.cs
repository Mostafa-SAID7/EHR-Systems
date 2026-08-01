using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.Security.AuditLogging;

/// <summary>
/// Interface for security audit logging.
/// Single responsibility: Security event logging contract.
/// </summary>
public interface ISecurityAuditLogger
{
    /// <summary>
    /// Log successful authentication.
    /// </summary>
    Task LogAuthenticationSuccessAsync(string userId, string method, string ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Log failed authentication attempt.
    /// </summary>
    Task LogAuthenticationFailureAsync(string username, string reason, string ipAddress, CancellationToken cancellationToken = default);

    /// <summary>
    /// Log authorization denial.
    /// </summary>
    Task LogAuthorizationDenialAsync(string userId, string resource, string reason, CancellationToken cancellationToken = default);

    /// <summary>
    /// Log password change.
    /// </summary>
    Task LogPasswordChangeAsync(string userId, bool success, CancellationToken cancellationToken = default);

    /// <summary>
    /// Log permission change.
    /// </summary>
    Task LogPermissionChangeAsync(string userId, string targetUserId, string changeDetails, CancellationToken cancellationToken = default);

    /// <summary>
    /// Log sensitive data access.
    /// </summary>
    Task LogSensitiveDataAccessAsync(string userId, string dataType, string action, CancellationToken cancellationToken = default);

    /// <summary>
    /// Log security event.
    /// </summary>
    Task LogSecurityEventAsync(string eventType, string userId, Dictionary<string, object>? details = null, CancellationToken cancellationToken = default);
}
