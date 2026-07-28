using System;
using Xunit;

namespace EHRPlatform.Tests.Contract;

/// <summary>
/// Base class for contract tests
/// </summary>
public abstract class ContractTestBase
{
    protected const string ApiVersion = "v1";

    protected string BuildServiceUrl(string serviceName, string endpoint)
    {
        return $"http://localhost:5000/{serviceName}/{ApiVersion}/{endpoint}";
    }

    protected Dictionary<string, string> GetContractHeaders(string consumerName = "")
    {
        return new Dictionary<string, string>
        {
            { "Content-Type", "application/json" },
            { "Accept", "application/json" },
            { "X-Consumer", consumerName.IsEmpty() ? "UnknownConsumer" : consumerName }
        };
    }

    protected void ValidateContractCompliance(object request, object expectedResponse)
    {
        Assert.NotNull(request);
        Assert.NotNull(expectedResponse);
    }
}

internal static class StringExtensions
{
    internal static bool IsEmpty(this string value) => string.IsNullOrWhiteSpace(value);
}
