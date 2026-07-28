using System;

namespace EHRPlatform.Tests;

/// <summary>
/// Central configuration for all test projects
/// </summary>
public static class TestConfiguration
{
    /// <summary>
    /// Base URL for API testing
    /// </summary>
    public static string ApiBaseUrl => Environment.GetEnvironmentVariable("TEST_API_BASE_URL") ?? "http://localhost:5000";

    /// <summary>
    /// Database connection string for integration tests
    /// </summary>
    public static string DatabaseConnectionString => 
        Environment.GetEnvironmentVariable("TEST_DB_CONNECTION_STRING") ?? 
        "Host=localhost;Port=5432;Database=ehr_test;Username=test_user;Password=test_password";

    /// <summary>
    /// Redis connection string for cache tests
    /// </summary>
    public static string RedisConnectionString => 
        Environment.GetEnvironmentVariable("TEST_REDIS_CONNECTION_STRING") ?? 
        "localhost:6379";

    /// <summary>
    /// RabbitMQ connection string for message queue tests
    /// </summary>
    public static string RabbitMqConnectionString => 
        Environment.GetEnvironmentVariable("TEST_RABBITMQ_CONNECTION_STRING") ?? 
        "amqp://guest:guest@localhost:5672/";

    /// <summary>
    /// Default timeout for async operations (seconds)
    /// </summary>
    public static int DefaultTimeoutSeconds => 
        int.TryParse(Environment.GetEnvironmentVariable("TEST_TIMEOUT_SECONDS"), out var timeout) 
            ? timeout 
            : 30;

    /// <summary>
    /// Test data retention flag (keep test data after tests)
    /// </summary>
    public static bool RetainTestData => 
        bool.TryParse(Environment.GetEnvironmentVariable("TEST_RETAIN_DATA"), out var retain) 
            && retain;

    /// <summary>
    /// Enable detailed logging
    /// </summary>
    public static bool EnableDetailedLogging => 
        bool.TryParse(Environment.GetEnvironmentVariable("TEST_DETAILED_LOGGING"), out var enable) 
            && enable;

    /// <summary>
    /// Performance test threshold (milliseconds)
    /// </summary>
    public static int PerformanceThresholdMs => 
        int.TryParse(Environment.GetEnvironmentVariable("TEST_PERF_THRESHOLD_MS"), out var threshold) 
            ? threshold 
            : 100;

    /// <summary>
    /// Admin user email for tests
    /// </summary>
    public static string AdminEmail => 
        Environment.GetEnvironmentVariable("TEST_ADMIN_EMAIL") ?? "admin@test.ehr.local";

    /// <summary>
    /// Admin user password for tests
    /// </summary>
    public static string AdminPassword => 
        Environment.GetEnvironmentVariable("TEST_ADMIN_PASSWORD") ?? "AdminPassword123!";

    /// <summary>
    /// Test user email
    /// </summary>
    public static string TestUserEmail => 
        Environment.GetEnvironmentVariable("TEST_USER_EMAIL") ?? "user@test.ehr.local";

    /// <summary>
    /// Test user password
    /// </summary>
    public static string TestUserPassword => 
        Environment.GetEnvironmentVariable("TEST_USER_PASSWORD") ?? "UserPassword123!";

    /// <summary>
    /// HIPAA compliance mode (strict validation)
    /// </summary>
    public static bool HipaaComplianceMode => 
        bool.TryParse(Environment.GetEnvironmentVariable("TEST_HIPAA_MODE"), out var hipaa) 
            && hipaa;

    /// <summary>
    /// Get configuration summary for logging
    /// </summary>
    public static string GetConfigurationSummary() => $@"
Test Configuration Summary:
- API Base URL: {ApiBaseUrl}
- Database: {MaskConnectionString(DatabaseConnectionString)}
- Redis: {RedisConnectionString}
- RabbitMQ: {MaskConnectionString(RabbitMqConnectionString)}
- Timeout: {DefaultTimeoutSeconds}s
- Retain Data: {RetainTestData}
- Detailed Logging: {EnableDetailedLogging}
- Performance Threshold: {PerformanceThresholdMs}ms
- HIPAA Mode: {HipaaComplianceMode}
";

    private static string MaskConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return "<not set>";

        // Mask password in connection strings
        return System.Text.RegularExpressions.Regex.Replace(
            connectionString, 
            @"(Password|password|pwd|Pwd)=([^;]+)", 
            "$1=****");
    }
}
