using Microsoft.Extensions.Logging;

namespace EHRPlatform.Observability.Logging;

/// <summary>
/// Interface for logger configuration and setup.
/// Single responsibility: Logger configuration contract.
/// </summary>
public interface ILoggerConfiguration
{
    /// <summary>
    /// Configure structured logging with Serilog.
    /// </summary>
    void ConfigureStructuredLogging();

    /// <summary>
    /// Set log level for category.
    /// </summary>
    void SetLogLevel(string category, LogLevel level);

    /// <summary>
    /// Enable file logging to specified path.
    /// </summary>
    void EnableFileLogging(string logPath);

    /// <summary>
    /// Enable console logging.
    /// </summary>
    void EnableConsoleLogging();

    /// <summary>
    /// Add custom sink.
    /// </summary>
    void AddCustomSink(string sinkName);
}
