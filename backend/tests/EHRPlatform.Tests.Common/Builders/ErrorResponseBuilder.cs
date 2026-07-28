using System;
using System.Collections.Generic;

namespace EHRPlatform.Tests.Common.Builders;

/// <summary>
/// Builder for error response objects in tests
/// </summary>
public class ErrorResponseBuilder
{
    private string _message = "";
    private string _code = "";
    private int _statusCode = 400;
    private List<string> _details = new();
    private string _traceId = "";

    public ErrorResponseBuilder WithMessage(string message)
    {
        _message = message;
        return this;
    }

    public ErrorResponseBuilder WithCode(string code)
    {
        _code = code;
        return this;
    }

    public ErrorResponseBuilder WithStatusCode(int code)
    {
        _statusCode = code;
        return this;
    }

    public ErrorResponseBuilder WithDetail(string detail)
    {
        _details.Add(detail);
        return this;
    }

    public ErrorResponseBuilder WithTraceId(string traceId)
    {
        _traceId = traceId;
        return this;
    }

    public ErrorResponseBuilder WithValidationError(string field, string error)
    {
        _details.Add($"{field}: {error}");
        return this;
    }

    public ErrorResponseBuilder BadRequest()
    {
        _statusCode = 400;
        _code = "BadRequest";
        return this;
    }

    public ErrorResponseBuilder Unauthorized()
    {
        _statusCode = 401;
        _code = "Unauthorized";
        return this;
    }

    public ErrorResponseBuilder Forbidden()
    {
        _statusCode = 403;
        _code = "Forbidden";
        return this;
    }

    public ErrorResponseBuilder NotFound()
    {
        _statusCode = 404;
        _code = "NotFound";
        return this;
    }

    public ErrorResponseBuilder InternalServerError()
    {
        _statusCode = 500;
        _code = "InternalServerError";
        return this;
    }

    public dynamic Build()
    {
        return new
        {
            StatusCode = _statusCode,
            Message = _message,
            Code = _code,
            Details = _details,
            TraceId = _traceId,
            Timestamp = DateTime.UtcNow
        };
    }

    private void Reset()
    {
        _message = "";
        _code = "";
        _statusCode = 400;
        _details = new();
        _traceId = "";
    }
}
