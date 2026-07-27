#nullable enable

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EHRPlatform.Common.Data.EntityTypeConfiguration;

/// <summary>
/// Helper methods for common EF Core property configurations.
/// Use for consistent property handling across services.
/// </summary>
public static class DataAnnotationHelper
{
    /// <summary>
    /// Configure a required string property with max length.
    /// </summary>
    public static PropertyBuilder<string> ConfigureRequired(
        this PropertyBuilder<string> builder,
        int maxLength = 255)
    {
        return builder
            .IsRequired()
            .HasMaxLength(maxLength);
    }

    /// <summary>
    /// Configure an optional string property with max length.
    /// </summary>
    public static PropertyBuilder<string?> ConfigureOptional(
        this PropertyBuilder<string?> builder,
        int maxLength = 255)
    {
        return builder
            .IsRequired(false)
            .HasMaxLength(maxLength);
    }

    /// <summary>
    /// Configure a currency/money property with correct precision.
    /// </summary>
    public static PropertyBuilder<decimal> ConfigureMoney(
        this PropertyBuilder<decimal> builder,
        int precision = 18,
        int scale = 2)
    {
        return builder
            .HasPrecision(precision, scale)
            .HasDefaultValue(0);
    }

    /// <summary>
    /// Configure a percentage property (0-100).
    /// </summary>
    public static PropertyBuilder<decimal> ConfigurePercentage(
        this PropertyBuilder<decimal> builder)
    {
        return builder
            .HasPrecision(5, 2)
            .HasDefaultValue(0);
    }

    /// <summary>
    /// Configure an email property with standard constraints.
    /// </summary>
    public static PropertyBuilder<string> ConfigureEmail(
        this PropertyBuilder<string> builder)
    {
        return builder
            .IsRequired()
            .HasMaxLength(255);
    }

    /// <summary>
    /// Configure a phone number property.
    /// </summary>
    public static PropertyBuilder<string> ConfigurePhoneNumber(
        this PropertyBuilder<string> builder)
    {
        return builder
            .IsRequired()
            .HasMaxLength(20);
    }

    /// <summary>
    /// Configure a medical record number (MRN) property.
    /// </summary>
    public static PropertyBuilder<string> ConfigureMRN(
        this PropertyBuilder<string> builder)
    {
        return builder
            .IsRequired()
            .HasMaxLength(20);
    }

    /// <summary>
    /// Configure an ICD-10 code property.
    /// </summary>
    public static PropertyBuilder<string> ConfigureICD10Code(
        this PropertyBuilder<string> builder)
    {
        return builder
            .IsRequired()
            .HasMaxLength(10);
    }

    /// <summary>
    /// Configure a CPT code property.
    /// </summary>
    public static PropertyBuilder<string> ConfigureCPTCode(
        this PropertyBuilder<string> builder)
    {
        return builder
            .IsRequired()
            .HasMaxLength(10);
    }

    /// <summary>
    /// Configure a status property (typically enum-like).
    /// </summary>
    public static PropertyBuilder<string> ConfigureStatus(
        this PropertyBuilder<string> builder)
    {
        return builder
            .IsRequired()
            .HasMaxLength(50);
    }

    /// <summary>
    /// Configure a JSON property for storing complex objects.
    /// </summary>
    public static PropertyBuilder<TProperty> ConfigureJson<TProperty>(
        this PropertyBuilder<TProperty> builder)
        where TProperty : class
    {
        return builder;
    }

    /// <summary>
    /// Configure a DateTime property with UTC kind.
    /// </summary>
    public static PropertyBuilder<DateTime> ConfigureUtcDateTime(
        this PropertyBuilder<DateTime> builder)
    {
        return builder;
    }

    /// <summary>
    /// Configure a nullable DateTime property with UTC kind.
    /// </summary>
    public static PropertyBuilder<DateTime?> ConfigureUtcDateTimeNullable(
        this PropertyBuilder<DateTime?> builder)
    {
        return builder;
    }

    /// <summary>
    /// Create a unique index on a property.
    /// </summary>
    public static void CreateUniqueIndex<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        string propertyName,
        string? indexName = null)
        where TEntity : class
    {
        var index = builder.HasIndex(propertyName).IsUnique();

        if (!string.IsNullOrEmpty(indexName))
            index.HasDatabaseName(indexName);
    }
}
