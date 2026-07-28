using System;
using System.Collections.Generic;

namespace EHRPlatform.Tests.Common.Builders;

/// <summary>
/// Builder for API request objects in tests
/// </summary>
public class RequestBuilder<T> where T : class, new()
{
    private T _request = new T();
    private Dictionary<string, string> _headers = new();
    private Dictionary<string, string> _queryParameters = new();

    public RequestBuilder<T> WithHeader(string key, string value)
    {
        _headers[key] = value;
        return this;
    }

    public RequestBuilder<T> WithAuthorizationHeader(string token)
    {
        _headers["Authorization"] = $"Bearer {token}";
        return this;
    }

    public RequestBuilder<T> WithContentType(string contentType)
    {
        _headers["Content-Type"] = contentType;
        return this;
    }

    public RequestBuilder<T> WithQueryParameter(string key, string value)
    {
        _queryParameters[key] = value;
        return this;
    }

    public RequestBuilder<T> WithJsonContentType()
    {
        return WithContentType("application/json");
    }

    public RequestBuilder<T> WithUserAgent(string userAgent)
    {
        _headers["User-Agent"] = userAgent;
        return this;
    }

    public (T Request, Dictionary<string, string> Headers, Dictionary<string, string> QueryParameters) Build()
    {
        var result = (Request: _request, Headers: _headers, QueryParameters: _queryParameters);
        Reset();
        return result;
    }

    private void Reset()
    {
        _request = new T();
        _headers = new();
        _queryParameters = new();
    }
}
