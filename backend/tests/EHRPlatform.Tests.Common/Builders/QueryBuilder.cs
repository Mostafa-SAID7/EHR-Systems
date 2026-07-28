using System;
using System.Collections.Generic;

namespace EHRPlatform.Tests.Common.Builders;

/// <summary>
/// Base builder for test queries
/// </summary>
public abstract class QueryBuilder<TQuery> where TQuery : class, new()
{
    protected TQuery Query { get; set; } = new TQuery();
    protected Dictionary<string, object> Parameters { get; } = new();

    public virtual TQuery Build()
    {
        var result = Query;
        Query = new TQuery();
        Parameters.Clear();
        return result;
    }

    public virtual QueryBuilder<TQuery> WithParameter(string key, object value)
    {
        Parameters[key] = value;
        return this;
    }

    public virtual QueryBuilder<TQuery> Reset()
    {
        Query = new TQuery();
        Parameters.Clear();
        return this;
    }
}
