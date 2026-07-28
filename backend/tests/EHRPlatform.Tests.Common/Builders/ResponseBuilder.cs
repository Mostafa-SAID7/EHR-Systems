using System;
using System.Collections.Generic;

namespace EHRPlatform.Tests.Common.Builders;

/// <summary>
/// Builder for API response objects in tests
/// </summary>
public class ResponseBuilder<T> where T : class, new()
{
    private T _response = new T();
    private Dictionary<string, object> _headers = new();
    private int _statusCode = 200;

    public ResponseBuilder<T> WithStatusCode(int code)
    {
        _statusCode = code;
        return this;
    }

    public ResponseBuilder<T> WithHeader(string key, object value)
    {
        _headers[key] = value;
        return this;
    }

    public ResponseBuilder<T> WithSuccessStatus()
    {
        _statusCode = 200;
        return this;
    }

    public ResponseBuilder<T> WithCreatedStatus()
    {
        _statusCode = 201;
        return this;
    }

    public ResponseBuilder<T> WithBadRequestStatus()
    {
        _statusCode = 400;
        return this;
    }

    public ResponseBuilder<T> WithUnauthorizedStatus()
    {
        _statusCode = 401;
        return this;
    }

    public ResponseBuilder<T> WithForbiddenStatus()
    {
        _statusCode = 403;
        return this;
    }

    public ResponseBuilder<T> WithNotFoundStatus()
    {
        _statusCode = 404;
        return this;
    }

    public ResponseBuilder<T> WithInternalServerErrorStatus()
    {
        _statusCode = 500;
        return this;
    }

    public (T Response, Dictionary<string, object> Headers, int StatusCode) Build()
    {
        var result = (Response: _response, Headers: _headers, StatusCode: _statusCode);
        Reset();
        return result;
    }

    private void Reset()
    {
        _response = new T();
        _headers = new();
        _statusCode = 200;
    }
}
