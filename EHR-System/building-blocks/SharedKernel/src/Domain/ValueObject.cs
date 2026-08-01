using System;
using System.Collections.Generic;
using System.Linq;

namespace EHRPlatform.SharedKernel.Domain;

/// <summary>
/// Base class for value objects.
/// Value objects are compared by value, not reference.
/// They are immutable and have no identity.
/// 
/// Example: Address is a value object (two addresses with same properties = equal).
/// Example: Patient is an entity (identity matters, even if all properties are same).
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Get the atomic values that comprise the value object.
    /// Used for equality comparison.
    /// 
    /// Example for Address:
    /// <code>
    /// protected override IEnumerable&lt;object&gt; GetAtomicValues()
    /// {
    ///     yield return Street;
    ///     yield return City;
    ///     yield return State;
    ///     yield return ZipCode;
    /// }
    /// </code>
    /// </summary>
    protected abstract IEnumerable<object?> GetAtomicValues();

    /// <summary>
    /// Compare two value objects for equality.
    /// </summary>
    public override bool Equals(object? obj)
    {
        if (obj is not ValueObject valueObject)
            return false;

        return GetAtomicValues().SequenceEqual(valueObject.GetAtomicValues());
    }

    /// <summary>
    /// Generic equality comparison.
    /// </summary>
    public bool Equals(ValueObject? other)
    {
        return Equals((object?)other);
    }

    /// <summary>
    /// Get hash code based on atomic values.
    /// </summary>
    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Aggregate(1, (current, obj) =>
            {
                unchecked
                {
                    return current * 23 + (obj?.GetHashCode() ?? 0);
                }
            });
    }

    /// <summary>
    /// Override == operator for value objects.
    /// </summary>
    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
            return false;

        return ReferenceEquals(left, null) || left.Equals(right);
    }

    /// <summary>
    /// Override != operator for value objects.
    /// </summary>
    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !(left == right);
    }
}
