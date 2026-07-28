using System;

namespace EHRPlatform.Tests.Common.Builders;

/// <summary>
/// Base builder for test entities
/// </summary>
public abstract class TestEntityBuilder<T> where T : class, new()
{
    protected T Entity { get; set; } = new T();

    public virtual T Build()
    {
        var result = Entity;
        Entity = new T();
        return result;
    }

    public virtual TestEntityBuilder<T> Reset()
    {
        Entity = new T();
        return this;
    }
}
