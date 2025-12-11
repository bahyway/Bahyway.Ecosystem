using System;
using System.Collections.Generic;
using System.Linq;

namespace BahyWay.SharedKernel.Domain.Primitives;

/// <summary>
/// Base class for value objects.
/// Value objects are immutable and defined by their attributes.
/// REUSABLE: ✅ ALL PROJECTS
/// EXAMPLES: Email, Money, Address, PhoneNumber, Coordinates
/// </summary>
public abstract class ValueObject : IEquatable<ValueObject>
{
    /// <summary>
    /// Gets the atomic values that define this value object.
    /// Used for equality comparison.
    /// </summary>
    protected abstract IEnumerable<object?> GetAtomicValues();

    public bool Equals(ValueObject? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        if (GetType() != other.GetType()) return false;

        return GetAtomicValues().SequenceEqual(other.GetAtomicValues());
    }

    public override bool Equals(object? obj)
    {
        return obj is ValueObject other && Equals(other);
    }

    public override int GetHashCode()
    {
        return GetAtomicValues()
            .Aggregate(default(HashCode), (hashCode, obj) =>
            {
                hashCode.Add(obj);
                return hashCode;
            })
            .ToHashCode();
    }

    public static bool operator ==(ValueObject? left, ValueObject? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(ValueObject? left, ValueObject? right)
    {
        return !Equals(left, right);
    }
}