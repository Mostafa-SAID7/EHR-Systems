#nullable enable

using EHRPlatform.Common.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EHRPlatform.Common.Data.EntityTypeConfiguration;

/// <summary>
/// Helper methods for configuring Value Objects in EF Core.
/// Use for consistent owned entity patterns across services.
/// </summary>
public static class ValueObjectConfigurationHelper
{
    /// <summary>
    /// Configure a value object as an owned entity (owned type).
    /// </summary>
    public static OwnedNavigationBuilder<TEntity, TValueObject> OwnsValueObject<TEntity, TValueObject>(
        this EntityTypeBuilder<TEntity> builder,
        System.Linq.Expressions.Expression<System.Func<TEntity, TValueObject?>> navigationExpression)
        where TEntity : class
        where TValueObject : ValueObject
    {
        return builder.OwnsOne(navigationExpression);
    }
}

