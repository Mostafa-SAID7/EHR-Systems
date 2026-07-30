#nullable enable

using System.Net;
using System.Text.Json;
using EHRPlatform.Common.Domain.Exceptions;
using EHRPlatform.Common.Shared.Responses;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Common.Shared.Middleware;

/// <summary>
/// Global exception handler that converts domain and infrastructure exceptions
/// into RFC 7807 Problem Details JSON responses.
///
/// Mapping:
///   ValidationException      → 422 Unprocessable Entity
///   NotFoundException        → 404 Not Found
///   UnauthorizedException    → 401 Unauthorized
///   ForbiddenException       → 403 Forbidden
///   ConflictException        → 409 Conflict
///   BusinessRuleException    → 422 Unprocessable Entity
///   HIPAAException           → 403 Forbidden  (never expose HIPAA detail to client)
///   ExternalServiceException → 502 Bad Gateway
///   Exceptions.TimeoutException → 504 Gateway Timeout
///   Everything else          → 500 Internal Server Error
///
/// HIPAA: Stack traces and inner exception messages are NEVER returned to clients.
/// Only the CorrelationId is echoed so ops can match the request to server logs.
/// </summary>
public sealed class GlobalExceptionMiddleware
{
    private readonly RequestDelegate     _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment    _env;

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented        = false
    };

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next   = next   ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _env    = env    ?? throw new ArgumentNullException(nameof(env));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString()
                            ?? context.TraceIdentifier;

        // Map exception to HTTP status + client-safe title
        var (statusCode, title, errorCode) = MapException(exception);

        // Log with appropriate severity
        if (statusCode >= 500)
            _logger.LogError(exception,
                "Unhandled exception [{ErrorCode}] CorrelationId={CorrelationId} Path={Path}",
                errorCode, correlationId, context.Request.Path);
        else
            _logger.LogWarning(
                "Domain exception [{ErrorCode}] CorrelationId={CorrelationId} Path={Path} Detail={Detail}",
                errorCode, correlationId, context.Request.Path, exception.Message);

        // Build problem details
        var problem = new ProblemDetails
        {
            Status        = statusCode,
            Title         = title,
            ErrorCode     = errorCode,
            CorrelationId = correlationId,
            // Only include developer detail in non-production environments
            Detail        = _env.IsProduction() ? null : exception.Message
        };

        context.Response.StatusCode  = statusCode;
        context.Response.ContentType = "application/problem+json";

        var body = JsonSerializer.Serialize(problem, _jsonOptions);
        await context.Response.WriteAsync(body);
    }

    private static (int StatusCode, string Title, string ErrorCode) MapException(Exception ex) =>
        ex switch
        {
            ValidationException     => (422, "Validation Error",          "VALIDATION_ERROR"),
            NotFoundException       => (404, "Resource Not Found",        "NOT_FOUND"),
            UnauthorizedException   => (401, "Unauthorized",              "UNAUTHORIZED"),
            ForbiddenException      => (403, "Forbidden",                 "FORBIDDEN"),
            HIPAAException          => (403, "Access Denied",             "ACCESS_DENIED"),   // Never expose HIPAA detail
            ConflictException       => (409, "Conflict",                  "CONFLICT"),
            BusinessRuleException   => (422, "Business Rule Violation",   "BUSINESS_RULE_VIOLATION"),
            ExternalServiceException=> (502, "Upstream Service Error",    "EXTERNAL_SERVICE_ERROR"),
            Domain.Exceptions.TimeoutException => (504, "Request Timeout",       "TIMEOUT"),
            _                       => (500, "An unexpected error occurred.", "INTERNAL_ERROR")
        };
}

/// <summary>Extension methods to register GlobalExceptionMiddleware.</summary>
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseEHRGlobalExceptionHandler(this IApplicationBuilder app)
        => app.UseMiddleware<GlobalExceptionMiddleware>();
}

