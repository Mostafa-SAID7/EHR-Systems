using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EHRPlatform.Observability.ErrorReporting;

/// <summary>
/// Interface for error reporting service (Sentry, Rollbar, etc).
/// Single responsibility: Report errors/exceptions to external service.
/// </summary>
public interface IErrorReporter
{
    /// <summary>
    /// Report exception with context.
    /// </summary>
    Task ReportExceptionAsync(Exception exception, ErrorContext? context = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Report error message.
    /// </summary>
    Task ReportErrorAsync(string message, ErrorSeverity severity = ErrorSeverity.Error, Dictionary<string, object>? details = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Report warning.
    /// </summary>
    Task ReportWarningAsync(string message, Dictionary<string, object>? details = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add breadcrumb for error context.
    /// </summary>
    Task AddBreadcrumbAsync(string message, BreadcrumbLevel level = BreadcrumbLevel.Info, string? category = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Set error context.
    /// </summary>
    void SetContext(string key, object value);

    /// <summary>
    /// Get is error reporter enabled.
    /// </summary>
    bool IsEnabled { get; }
}
